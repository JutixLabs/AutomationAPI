using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleSheetsController : ControllerBase
    {
        private readonly IGoogleSheetsService _googleSheetsService;
        public GoogleSheetsController(IGoogleSheetsService googleSheetsService)
        {
            _googleSheetsService = googleSheetsService;
        }

        [Authorize]
        [HttpGet("rows")]
        public async Task<IActionResult> GetRows(string spreedSheetId, string range = "Sheet1")
        {
            var rows = await _googleSheetsService.GetRowsAsync(spreedSheetId, range);
            return Ok(rows);
        }
    }
}
