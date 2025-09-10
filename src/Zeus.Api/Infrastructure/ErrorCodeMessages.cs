namespace Zeus.Api.Infrastructure
{
    public class ErrorCodeMessages
    {

        private readonly Dictionary<string, ErrorCodeInfo> _errorMessages = new()
        {
            #region Request
            {
                ErrorCodes.REQUEST_ID_INVALID,
                new ErrorCodeInfo(title: "Invalid Id", detail: "The value of the id parameter is not in valid format.")
            },
            {
                ErrorCodes.REQUEST_IP_INVALID,
                new ErrorCodeInfo(title: "Invalid Ip Address", detail: "Missing or invalid ip address.")
            },
            {
                ErrorCodes.REQUEST_RATE_LIMIT_EXCEEDED,
                new ErrorCodeInfo(title: "Rate Limit Exceed", detail: "You have exceeded the allowed number of request per hour.")
            },
            #endregion

            #region Server
            {
                ErrorCodes.INTERNAL_SERVER_ERROR,
                new ErrorCodeInfo(title: "Internal Server Error", detail: "An unknown internal server error occured.")
            },
            {
                ErrorCodes.SERVER_COULD_NOT_HANDLE_REQUEST,
                new ErrorCodeInfo(title: "Internal Server Error", detail: "The server could not handle this request.")
            }
            #endregion
        };

        /// <summary>
        /// Gets the erro code information based on the error code.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <returns>the error code information, or null if the error code is not found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorCodeInfo GetErrorCodeInfo(string errorCode)
        {
            var info = _errorMessages.TryGetValue(errorCode, out var errorCodeInfo) ? errorCodeInfo : null;
            return info ?? throw new ArgumentNullException($"No error code info available for {errorCode}");
        }
    }
}
