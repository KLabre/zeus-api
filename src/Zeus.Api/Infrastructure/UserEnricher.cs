using Serilog.Core;
using Serilog.Events;
using System.Security.Claims;

namespace Zeus.Api.Infrastructure
{
    public class UserEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserEnricher() : this(new HttpContextAccessor()) { }

        public UserEnricher(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.
                    CreateProperty("UserId", _contextAccessor.HttpContext?.User
                        .FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous")
                    );
            logEvent.AddPropertyIfAbsent(
                propertyFactory.
                    CreateProperty("UserEmail", _contextAccessor.HttpContext?.User
                        .FindFirstValue(ClaimTypes.Email) ?? "anonymous")
                    );
        }
    }
}
