using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotionWebhookController : ControllerBase
    {
        private readonly ITriggerEngineService _triggerEngine;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly ILogger<NotionWebhookController> _logger;

        public NotionWebhookController(ITriggerEngineService triggerEngine, AppDbContext dbContext,
            IConfiguration config, ILogger<NotionWebhookController> logger)
        {
            _triggerEngine = triggerEngine;
            _dbContext = dbContext;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            // One-time verification handshake — Notion sends this exactly once when the
            // subscription is created. Log the token clearly so it can be copied into
            // appsettings and pasted back into the Notion dashboard's "Verify" step.
            if (rawBody.Contains("verification_token") && !Request.Headers.ContainsKey("X-Notion-Signature"))
            {
                _logger.LogWarning("[NOTION WEBHOOK VERIFICATION] Copy this token into appsettings.json " +
                    "under NotionOAuth:WebhookSecret, AND paste it into the Notion integration's Webhooks tab " +
                    "to complete verification. Raw payload: {Body}", rawBody);
                return Ok();
            }

            var webhookSecret = _config["NotionOAuth:WebhookSecret"];

            if (!string.IsNullOrEmpty(webhookSecret) && Request.Headers.TryGetValue("X-Notion-Signature", out var signatureHeader))
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
                var computedSignature = "sha256=" + Convert.ToHexString(computedHash).ToLower();

                if (computedSignature != signatureHeader.ToString())
                {
                    _logger.LogError("[ERROR] Notion webhook signature verification failed.");
                    return BadRequest("Invalid signature.");
                }
            }
            else
            {
                _logger.LogWarning("[WARN] Notion webhook received without signature verification configured.");
            }

            dynamic evt = JsonConvert.DeserializeObject(rawBody);
            string workspaceId = evt.workspace_id;
            string eventType = evt.type;

            var connectedApp = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.Provider == "notion" && x.ExternalAccountId == workspaceId);

            if (connectedApp == null)
            {
                _logger.LogInformation("[INFO] Notion webhook received for unknown workspace: {WorkspaceId}", workspaceId);
                return Ok();
            }

            var triggerName = eventType switch
            {
                "page.content_updated" => "notion.page_updated",
                "page.properties_updated" => "notion.page_updated",
                "comment.created" => "notion.comment_added",
                _ => null
            };

            if (triggerName != null)
            {
                await _triggerEngine.ExecuteTriggerAsync(new TriggerEvent
                {
                    TriggerName = triggerName,
                    UserId = connectedApp.UserId,
                    Payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawBody)
                });
            }

            return Ok();
        }
    }
}
