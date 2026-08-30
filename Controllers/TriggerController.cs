using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        private readonly IAutomationService _automationService;
        private readonly TriggerDefinitionRegistry _triggerRegistry;
        private readonly ITriggerEngineService _triggerEngine;
        public TriggerController(IAutomationService automationService, TriggerDefinitionRegistry triggerRegistry, ITriggerEngineService triggerEngine)
        { 
            _automationService = automationService; 
            _triggerRegistry = triggerRegistry;
            _triggerEngine = triggerEngine;
        }

        [HttpGet("GetTriggers")]
        public IActionResult GetTriggers()
        {
            var triggers = _triggerRegistry.GetTriggers();
            return Ok(triggers);
        }

        [Authorize]
        [HttpPost("fire")]
        public async Task<IActionResult> FireTrigger([FromBody] TriggerEvent triggerEvent)
        {
            // Always use the caller's own identity — never trust a userId the client sent,
            // or this becomes a way to fire other users' workflows.
            triggerEvent.UserId = User.GetLoggedInUserId();

            await _triggerEngine.ExecuteTriggerAsync(triggerEvent);
            return Ok(new
            {
                message = "Trigger executed successfully"
            });
        }

        [Authorize]
        [HttpPost("ExecuteTrigger/{eventName}")]
        public async Task<IActionResult> ExecuteTrigger(string eventName)
        {
            var result = await _automationService.ExecuteTrigger(eventName);
            return Ok(new { Trigger = eventName, Logs = result });
        }
    }
}
