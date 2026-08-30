using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomationController : ControllerBase
    {
        private readonly IAutomationService _automationService;
        public AutomationController(IAutomationService automationService)
        {
            _automationService = automationService;
        }

        [Authorize]
        [HttpPost("WorkFlow")]
        public async Task<IActionResult> CreateRule(CreateWorkFlow rule)
        {
            await _automationService.CreatRuleAsync(rule);
            return Ok(rule);
        }

        [Authorize]
        [HttpGet("GetAllRules")]
        public async Task<IActionResult> GetAllRules()
        {
            var rules = await _automationService.GetAllRulesAsync();
            return Ok(rules);
        }

        [Authorize]
        [HttpDelete("DeleteAllRules")]
        public async Task<IActionResult> DeleteAllRules()
        {
            var rules = await _automationService.DeleteAllRulesAsync();
            return Ok(rules);
        }

        [Authorize]
        [HttpDelete("DeleteRule/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            var result = await _automationService.DeleteByIdAsync(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("ToggleRule/{id}")]
        public async Task<IActionResult> ToggleRule(int id)
        {
            await _automationService.ToggleRuleAsync(id);
            return Ok();
        }

        
    }
}