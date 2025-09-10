using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Zeus.Api.Models;
using Zeus.Api.Models.Resources;

namespace Zeus.Api.Filters
{
    /// <summary>
    /// Centralized the Success/Problem response in all Controllers
    /// </summary>
    public class ServiceResultResponseFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                // Check if the result is of type ServiceResult<T>
                var serviceResultType = objectResult.Value?.GetType();
                if (serviceResultType != null && serviceResultType.IsGenericType &&
                    serviceResultType.GetGenericTypeDefinition() == typeof(ServiceResult<>))
                {
                    // Extract the service result (generic type argument T)
                    var serviceResult = objectResult.Value as dynamic;
                    if(serviceResult != null)
                    {
                        if (serviceResult.IsSuccess)
                        {
                            // On success, return the Data
                            objectResult.StatusCode = 200;
                            objectResult.Value = serviceResult.Data;
                        }
                        else
                        {
                            // On error, return the ProblemDetails
                            var problemDetails = serviceResult.Problem ?? new CustomProblemDetails
                            {
                                Status = 500, // Default to 500 if no status is set
                                Title = "An unknown error occurred",
                                Detail = "An unexpected error occurred. Please try again later."
                            };
                            objectResult.StatusCode = problemDetails.Status ?? 500;
                            objectResult.Value = problemDetails;
                        }
                    }
                }
            }

            // Proceed with the next action filter (if any)
            await next();
        }
    }
}
