using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GitHubWebhookController : ControllerBase
    {
        private readonly ITriggerEngineService _triggerEngine;
        public GitHubWebhookController(ITriggerEngineService triggerEngine)
        {
            _triggerEngine = triggerEngine;
        }

        [HttpPost]
        public async Task<IActionResult> Resolve()
        {
            var evenType = Request.Headers["X-GitHub-Event"];

            using var reader = new StreamReader(Request.Body);

            var body = await reader.ReadToEndAsync();

            if (evenType == "issues")
            {
                await _triggerEngine.ExecuteTriggerAsync(
                    new TriggerEvent
                    {
                        TriggerName = "github.issue_created",
                        UserId = "github",
                        Payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(body)
                    });
            }

            return Ok();
        }
    }
}
