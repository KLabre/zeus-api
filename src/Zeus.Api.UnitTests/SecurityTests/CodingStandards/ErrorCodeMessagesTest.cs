using Zeus.Api.Infrastructure;
using Zeus.Api.Models;

namespace Zeus.Api.UnitTests.SecurityTests.CodingStandards
{
    public class ErrorCodeMessagesTest
    {
        /// <summary>
        /// Ensures we have populated all information about error codes
        /// 
        /// RATIONALE: Every error code must have a title and a description so our clients can navigate solutions
        /// </summary>
        [Fact]
        public void ErrorCodeMessages_ByConvention_AllErrorCodesShouldBeMappedToErrorCodeInfo()
        {
            // Arrange
            var errorCodeMessages = new ErrorCodeMessages();

            // Act & Assert
            foreach (var field in typeof(ErrorCodes).GetFields())
            {
                var errorCode = field.GetValue(null)?.ToString();
                Assert.NotNull(errorCode);
                
                var errorCodeInfo = errorCodeMessages.GetErrorCodeInfo(errorCode);
                Assert.NotNull(errorCodeInfo);
                Assert.NotNull(errorCodeInfo.Title);
                Assert.NotNull(errorCodeInfo.Detail);
                Assert.NotEqual("", errorCodeInfo.Title);
                Assert.NotEqual("", errorCodeInfo.Detail);
            }
        }
    }
}
