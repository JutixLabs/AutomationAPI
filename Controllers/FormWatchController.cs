using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormWatchController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public FormWatchController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create(FormWatch model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.LastProcessedRow = 1;

            _dbContext.FormWatches.Add(model);

            await _dbContext.SaveChangesAsync();

            return Ok(model);
        }
    }
}
