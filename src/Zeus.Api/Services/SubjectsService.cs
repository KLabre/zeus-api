using Ardalis.GuardClauses;
using ILogger = Serilog.ILogger;
using Serilog;
using Zeus.Api.Models.Resources;
using Zeus.Api.Infrastructure;
using AutoMapper;
using Zeus.Api.Models.Entities;

namespace Zeus.Api.Services
{
    public interface ISubjectsService
    {
        public ServiceResult<SubjectsResponse> GetSubjects();
    }

    public class SubjectsService : ServiceBase, ISubjectsService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public SubjectsService(
            ErrorCodeMessages errorCodeMessages,
            ILogger logger,
            IMapper mapper
            ) : base(errorCodeMessages)
        {
            Guard.Against.Null(errorCodeMessages, nameof(errorCodeMessages));
            Guard.Against.Null(logger, nameof(logger));
            Guard.Against.Null(mapper, nameof(mapper));

            _logger = Log.ForContext<RootService>();
            _mapper = mapper;
        }

        public ServiceResult<SubjectsResponse> GetSubjects()
        {
            try
            {
                var entity = new SubjectsEntity();
                var resource = _mapper.Map<SubjectsResponse>(entity);
                return ServiceResult<SubjectsResponse>.Success(resource);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not handle GetSubjects");
                throw;
            }
        }
    }
}
