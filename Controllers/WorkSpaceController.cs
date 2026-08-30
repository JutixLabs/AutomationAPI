using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkSpaceController : ControllerBase
    {
        private readonly IWorkSpaceService _workSpaceService;
        public WorkSpaceController(IWorkSpaceService workSpaceService)
        {
            _workSpaceService = workSpaceService;
        }

        [Authorize]
        [HttpPost("CreateWorkSpace")]
        public async Task<IActionResult> CreateWorkSpace([FromBody] CreateWorkSpace model)
        {
            var result = await _workSpaceService.CreatetWorkSpaceAsync(model);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("CreateFolder/{workSpaceId}")]
        public async Task<IActionResult> CreateFolder(int workSpaceId, [FromBody] CreateFolder model)
        {
            var result = await _workSpaceService.CreateFolder(workSpaceId, model);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("GetWorkSpace")]
        public async Task<IActionResult> GetWorkSpace()
        {
            var result = await _workSpaceService.GetWorkSpaceAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("GetAllFolders/{workSpaceId}")]
        public async Task<IActionResult> GetAllFolders(int workSpaceId)
        {
            var result = await _workSpaceService.GetAllFoldersAsync(workSpaceId);
            return Ok(result);
        }
    }
}
