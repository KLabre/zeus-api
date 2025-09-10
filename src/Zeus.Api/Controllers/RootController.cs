using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Zeus.Api.Models;
using Zeus.Api.Models.Resources;
using Zeus.Api.Services;

namespace Zeus.Api.Controllers
{
    //[Authorize]
    [Route("/")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class RootController : ControllerBase
    {
        private readonly IRootService _service;

        public RootController(IRootService rootService)
        {
            Guard.Against.Null(rootService, nameof(rootService));

            _service = rootService;
        }

        // GET /
        [HttpGet(Name = nameof(GetRoot))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RootResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomProblemDetails))]
        public IActionResult GetRoot()
        {
            return Ok(_service.GetRoot());
        }
    }
}
