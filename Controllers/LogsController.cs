using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IExecutionLogService _executionLogService;
        public LogsController(IExecutionLogService executionLogService)
        {
            _executionLogService = executionLogService;
        }

        [Authorize]
        [HttpGet("GetLogs")]
        public async Task<IActionResult> Get()
        {
            var logs = await _executionLogService.GetExecutionLogAsync();
            return Ok(logs); 
        }

        [Authorize]
        [HttpDelete("DeleteAllLogs")]
        public async Task<IActionResult> Delete()
        {
            var deletedLogs = await _executionLogService.DeleteAllLogsAsync();
            return Ok(deletedLogs);
        }
    }
}
