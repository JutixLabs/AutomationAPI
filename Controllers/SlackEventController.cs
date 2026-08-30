using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlackEventController : ControllerBase
    {
        private readonly ITriggerEngineService _triggerEngine;
        public SlackEventController(ITriggerEngineService triggerEngine)
        {
            _triggerEngine = triggerEngine;
        }

        [HttpPost("Events")]
        public async Task<IActionResult> Receive([FromBody] SlackEventRequest request)
        {
            if (request.Type == "url_verification")
            {
                return Ok(request.Challenge);
            }

            if (request.Event?.Type == "message")
            {
                await _triggerEngine.ExecuteTriggerAsync(
                    new TriggerEvent
                    {
                        TriggerName = "slack.new_message",
                        UserId = request.Event.User,
                        Payload = new Dictionary<string, object>
                        {
                           { "message", request.Event.Text },
                           { "channel", request.Event.Channel },
                           { "user", request.Event.User }
                        }
                    });

            }

            return Ok();
        }
    }
}
