using Ardalis.GuardClauses;
using Zeus.Api.Extensions;


namespace Zeus.Api.Services
{
    public interface IContextAccessorService
    {
        Guid? GetUserId();
        string? GetUserEmail();
        string? GetHeader(string key);
        string? GetQueryParameter(string key);
    }

    public class ContextAccessorService : IContextAccessorService
    {
        private readonly IHttpContextAccessor _context;

        public ContextAccessorService(IHttpContextAccessor context)
        {
            Guard.Against.Null(context, nameof(context));

            _context = context;
        }

        public Guid? GetUserId()
        {
            var userId = _context.HttpContext?.User.Identity?.GetUserId();
            if (userId != null)
            {
                try
                {
                    return Guid.Parse(userId);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public string? GetUserEmail()
        {
            return _context.HttpContext?.User.Identity?.GetUserEmail();
        }


        public string? GetQueryParameter(string key)
        {
            var queryParams = _context.HttpContext?.Request.Query;
            if (queryParams == null || !queryParams.ContainsKey(key))
                return null;
            return queryParams[key].ToString();
        }

        public string? GetHeader(string key)
        {
            var headers = _context.HttpContext?.Request.Headers;
            if (headers == null || !headers.ContainsKey(key)) return null;
            return headers[key].ToString();
        }
    }
}
