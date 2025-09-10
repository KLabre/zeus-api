using Microsoft.AspNetCore.Mvc;

namespace Zeus.Api.Models
{
    public class CustomProblemDetails : ProblemDetails
    {
        public string? ErrorCode { get; set; }
    }
}
