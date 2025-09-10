using Ardalis.GuardClauses;

// Resharper disbale IncosistentNaming
// This file should be ignored because we will eventually generate documentation automatically for these codes
namespace Zeus.Api.Infrastructure
{
    /// <summary>
    /// Represents dertailed information for an error code
    /// </summary>
    public class ErrorCodeInfo
    {
        public ErrorCodeInfo(string title, string detail)
        {
            Guard.Against.Null(title, nameof(title));
            Guard.Against.Null(detail, nameof(detail));

            Title = title;
            Detail = detail;
        }

        /// <summary>
        /// The title or brief name of the error.
        /// Should be simpe e.g.: 'Invalid Email'
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The detailed message associated with the error code.
        /// Goes without saying e.g.: 'The provided email is not in a valid format'
        /// </summary>
        public string Detail { get; set; }
    }

    /// <summary>
    /// Contains predefined error codes for the application
    /// </summary>
    public class ErrorCodes
    {
        #region
        /// <summary>
        /// The error code for when the request id parameter is an invalid format.
        /// </summary>
        public const string REQUEST_ID_INVALID = "REQUEST_ID_INVALID";

        /// <summary>
        /// the error code for an invalid client ip address
        /// </summary>
        public const string REQUEST_IP_INVALID = "REQUEST_IP_INVALID";

        /// <summary>
        /// The error code for when a client exceeds the limit of requests given a period of time.
        /// </summary>
        public const string REQUEST_RATE_LIMIT_EXCEEDED = "REQUEST_RATE_LIMIT_EXCEEDED";
        #endregion

        #region
        /// <summary>
        /// The error code for an unkown internal server error.
        /// </summary>
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";

        /// <summary>
        /// the error code for whne the server could not handle the request given a gap in configuration or logic
        /// </summary>
        public const string SERVER_COULD_NOT_HANDLE_REQUEST = "SERVER_COULD_NOT_HANDLE_REQUEST";
        #endregion
    }
}
