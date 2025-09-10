using Ardalis.GuardClauses;
using Serilog;
using Zeus.Api.Controllers;
using Zeus.Api.Infrastructure;
using Zeus.Api.Models.Resources;
using ILogger = Serilog.ILogger;

namespace Zeus.Api.Services
{
    public interface IRootService
    {
        public ServiceResult<RootResponse> GetRoot();
    }

    public class RootService : ServiceBase, IRootService
    {
        private readonly ILogger _logger;

        public RootService(
            ErrorCodeMessages errorCodeMessages,
            ILogger logger
            ) : base(errorCodeMessages)
        {
            Guard.Against.Null(errorCodeMessages, nameof(errorCodeMessages));
            Guard.Against.Null(logger, nameof(logger));

            _logger = Log.ForContext<RootService>();
        }

        public ServiceResult<RootResponse> GetRoot()
        {
            try
            {
                var resource = new RootResponse
                {
                    Self = Link.To(nameof(RootController.GetRoot)),
                    Subjects = Link.To(nameof(SubjectsController.GetSubjects))
                };
                return ServiceResult<RootResponse>.Success(resource);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not handle GetRoot");
                throw;
            }
        }
    }
}
