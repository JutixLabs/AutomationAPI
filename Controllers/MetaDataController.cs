using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaDataController : ControllerBase
    {
        private readonly IMetadataService _metadataService;
        public MetaDataController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }
        [HttpGet("triggers")]
        public IActionResult GetTriggers()
        {
            var triggers = new List<string>
            {
                "manual",
                "user_registered",
                "error_occurred",
                "payment_success"
            };

            return Ok(triggers);
        }

        [HttpGet("actions/{provider}")]
        public IActionResult GetActions(string provider)
        {
            return Ok(_metadataService.GetActions(provider));
        }

        [HttpGet("providers")]
        public IActionResult GetProviders()
        {
            return Ok(_metadataService.GetProviders());
        }

        [HttpGet("resources/{provider}")]
        public IActionResult GetResource(string provider)
        {
            return Ok(_metadataService.GetResources(provider));
        }
    }
}
