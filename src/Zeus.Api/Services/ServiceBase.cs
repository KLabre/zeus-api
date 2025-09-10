using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using Zeus.Api.Models;
using System.Reflection;
using Zeus.Api.Infrastructure;

namespace Zeus.Api.Services
{
    public class ServiceBase
    {
        protected readonly ErrorCodeMessages _errorCodeMessages;

        protected ServiceBase(ErrorCodeMessages errorCodeMessages)
        {
            Guard.Against.Null(errorCodeMessages, nameof(errorCodeMessages));

            _errorCodeMessages = errorCodeMessages;
        }

        public readonly JsonSerializerSettings JsonOptions = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented,
        };

        protected async Task<ContentResult> CreateErrorResponseMessage (HttpStatusCode statusCode, string? errorCode = null, string contentType = "application/json")
        {
            var code = (int)statusCode;
            var response = new ContentResult
            {
                StatusCode = code,
                ContentType = contentType
            };

            // We only need content when detail is present
            if (errorCode == null) return response;

            var errorCodeInfo = _errorCodeMessages.GetErrorCodeInfo(errorCode);
            var error = new CustomProblemDetails
            {
                Status = code,
                ErrorCode = errorCode,
                Title = errorCodeInfo.Title,
                Detail = errorCodeInfo.Detail
            };

            var content = new StringContent(JsonConvert.SerializeObject(error, JsonOptions));
            response.Content = await content.ReadAsStringAsync();
            return response;
        }

        protected async Task<ContentResult> CreateResponse(HttpStatusCode statusCode, dynamic? obj = null)
        {
            var code = (int)statusCode;
            var response = new ContentResult
            {
                StatusCode = code,
                ContentType = "application/json"
            };

            // We only need content when detail is present
            if (obj == null) return response;
            var content = new StringContent(JsonConvert.SerializeObject(obj, JsonOptions));
            response.Content = await content.ReadAsStringAsync();
            return response;
        }

        /// <summary>
        /// Extremely helpful when dealing with Salesforce queries to avoid using SELECT Fields(All) which enforces LIMIT 200
        /// So we can automatically transform all our properties into a select query.
        /// </summary>
        /// <param name="objectType">The type of the entity</param>
        /// <param name="propertiesToExclude">Property names that we don't want to select</param>
        /// <returns></returns>
        protected static string BuildSelectFieldsFor(Type objectType, string[] propertiesToExclude)
        {
            // Use reflection to get properties of the object type
            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Filter properties based on the JsonProperty attribute of name
            var selectedProperties = properties
                .Where(p =>
                {
                    // Get the JsonProperty attribute
                    var jsonProperty = p.GetCustomAttribute<JsonPropertyAttribute>();
                    var jsonPropertyName = jsonProperty?.PropertyName ?? p.Name;

                    // Exclude properties that match the given list
                    return !propertiesToExclude.Contains(p.Name) && !propertiesToExclude.Contains(jsonPropertyName);
                })
                .Select(p =>
                {
                    // Use JsonProperty name if it exists, otherwise fall back to the property name
                    var jsonProperty = p.GetCustomAttribute<JsonPropertyAttribute>();
                    return jsonProperty?.PropertyName ?? p.Name;
                });

            // Build the SELECT query by joingin the properties
            return string.Join(",", selectedProperties);
        }
    }
}
