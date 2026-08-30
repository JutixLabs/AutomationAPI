using AutomationAPI.DATA;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeadLetterController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IWorkflowExecutionService _workflowExecutionService;
        public DeadLetterController(AppDbContext dbContext, IWorkflowExecutionService workflowExecutionService)
        {
            _dbContext = dbContext;
            _workflowExecutionService = workflowExecutionService;
        }

        [HttpPost("retry/{id}")]
        public async Task<IActionResult> Retry(int id)
        {
            var deadLetter = await _dbContext.WorkflowDeadLetters.FindAsync(id);
            if (deadLetter == null)
            {
                return NotFound();
            }

            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(deadLetter.PayloadJson);

            await _workflowExecutionService.ExecuteRuleAsync(deadLetter.RuleId, payload);

            return Ok(new
            {
                message = "Workflow execution retried successfully"
            });
        }
    }
}
