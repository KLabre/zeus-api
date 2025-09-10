using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Zeus.Api.Models;
using Zeus.Api.Models.Resources;
using Zeus.Api.Services;

namespace Zeus.Api.Controllers
{
    //[Authorize]
    [Route("/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectsService _service;

        public SubjectsController(
            ISubjectsService service
            )
        {
            Guard.Against.Null(service, nameof(service));

            _service = service;
        }

        // GET /subjects
        [HttpGet(Name = nameof(GetSubjects))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubjectsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomProblemDetails))]
        public IActionResult GetSubjects()
        {
            return Ok(_service.GetSubjects());
        }
    }
}
