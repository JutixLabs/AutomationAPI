using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceProviderResolver _resolver;
        public ResourcesController(IResourceProviderResolver resolver)
        {
            _resolver = resolver;
        }

        [HttpGet("{provider}/{resourceType}")]
        public async Task<IActionResult> GetResources(string provider, string resourceType,
            [FromQuery] string serverId = null, [FromQuery] string boardId = null)
        {
            var p = _resolver.Resolve(provider);

            Dictionary<string, string> filters = null;
            if (!string.IsNullOrEmpty(serverId))
                filters = new Dictionary<string, string> { { "serverId", serverId } };
            if (!string.IsNullOrEmpty(boardId))
                filters = new Dictionary<string, string> { { "boardId", boardId } };

            return Ok(await p.GetResourcesAsync(resourceType, filters));
        }
    }
}
