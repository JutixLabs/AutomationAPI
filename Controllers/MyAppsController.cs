using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyAppsController : ControllerBase
    {
        private readonly IConnectAppsService _connectAppsService;
        public MyAppsController(IConnectAppsService connectAppsService)
        {
            _connectAppsService = connectAppsService;
        }

        [Authorize]
        [HttpGet("GetMyApps")]
        public async Task<IActionResult> GetMyApps()
        {
            var result = await _connectAppsService.GetConnectedAppsAsync();
            return Ok(result);
        }
        
        [Authorize]
        [HttpPost("ConnectApp")]
        public async Task<IActionResult> ConnectApp([FromBody] ConnectAppDTO model)
        {
            var result = await _connectAppsService.ConnectAppAsync(model);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("DisconnectApp/{appId}")]
        public async Task<IActionResult> DisconnectApp(int appId)
        {
            var result = await _connectAppsService.DeactivateAppAsync(appId);
            return Ok(result); 
        }
    }
}
