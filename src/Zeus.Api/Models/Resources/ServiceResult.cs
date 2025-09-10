namespace Zeus.Api.Models.Resources
{
    /// <summary>
    /// Constrains:
    /// <br />
    /// Our service layer should return domain or DTO objects,
    /// and let the controller or a middleware/filter handle the HTTP-specific
    /// translation(status codes, content negotiation, etc.).
    /// <br />
    /// <br />
    /// 
    /// Goals:
    /// <br />
    /// - Automatically populate a property(Href) on responses(controlled by the LinkRewritingFilter), which inherit from a base type, but not exposed publicly(e.g.Self → Href).
    /// <br />
    /// - Conditionally return error-related information
    /// <br />
    /// <br />
    /// 
    /// Solution:
    /// <br />
    /// - Introduce a unified result wrapper that encapsulates both success and error scenarios
    /// <br />
    /// <br />
    /// 
    /// Reasoning:
    /// <br />
    /// This way, the service can remain agnostic to HTTP specifics, while allowing the controller to decide
    /// how to map the service result into a proper HTTP response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceResult<T>
    {
        public T? Data { get; set; }
        public CustomProblemDetails? Problem { get; set; }
        public bool IsSuccess => Problem == null; // Success if no ProblemDetails

        // Factory method for success
        public static ServiceResult<T> Success(T data) => new() { Data = data };

        // Factory method for error
        public static ServiceResult<T> Error(CustomProblemDetails problem) => new() { Problem = problem };
    }
}
