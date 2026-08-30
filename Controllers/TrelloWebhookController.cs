using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrelloWebhookController : ControllerBase
    {
        private readonly ITriggerEngineService _triggerEngine;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TrelloWebhookController> _logger;

        public TrelloWebhookController(ITriggerEngineService triggerEngine, AppDbContext dbContext,
            ILogger<TrelloWebhookController> logger)
        {
            _triggerEngine = triggerEngine;
            _dbContext = dbContext;
            _logger = logger;
        }

        // Trello sends a HEAD request first to confirm the endpoint is reachable
        // before it will let you create the webhook.
        [HttpHead]
        public IActionResult Verify() => Ok();

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(rawBody))
                return Ok(); // Trello also sometimes pings with an empty body — just acknowledge.

            dynamic evt = JsonConvert.DeserializeObject(rawBody);
            string boardId = evt.model?.id;
            string actionType = evt.action?.type;

            if (string.IsNullOrEmpty(boardId))
                return Ok();

            var watched = await _dbContext.TrelloWatchedBoards
                .FirstOrDefaultAsync(b => b.BoardId == boardId);

            if (watched == null)
            {
                _logger.LogInformation("[INFO] Trello webhook received for unwatched board: {BoardId}", boardId);
                return Ok();
            }

            var triggerName = actionType switch
            {
                "createCard" => "trello.card_created",
                "commentCard" => "trello.comment_added",
                "updateCard" => evt.action?.data?.listAfter != null ? "trello.card_moved" : null,
                _ => (string)null
            };

            if (triggerName != null)
            {
                await _triggerEngine.ExecuteTriggerAsync(new TriggerEvent
                {
                    TriggerName = triggerName,
                    UserId = watched.UserId,
                    Payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawBody)
                });
            }

            return Ok();
        }
    }
}
