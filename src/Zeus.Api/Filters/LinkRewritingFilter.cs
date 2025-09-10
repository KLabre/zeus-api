using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Zeus.Api.Infrastructure;
using Zeus.Api.Models.Resources;
using System.Reflection;
using Zeus.Api.Extensions;

namespace Zeus.Api.Filters
{
    public class LinkRewritingFilter(IUrlHelperFactory urlHelperFactory) : IAsyncResultFilter
    {
        private readonly IUrlHelperFactory _urlHelperFactory = urlHelperFactory;

        public Task OnResultExecutionAsync(
            ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var asObjectResult = context.Result as ObjectResult;
            bool shouldSkip = asObjectResult?.StatusCode >= 400
                || asObjectResult?.Value == null
                || asObjectResult?.Value as Resource == null;

            if (shouldSkip) return next();

            var rewriter = new LinkRewriter(_urlHelperFactory.GetUrlHelper(context));
            RewriteAllLinks(asObjectResult!.Value!, rewriter);

            return next();
        }

        private static void RewriteAllLinks(object model, LinkRewriter rewriter)
        {
            if (model == null) return;

            var allProperties = model
                .GetType().GetTypeInfo()
                .GetAllProperties()
                .Where(p => p.CanRead)
                .ToArray();

            var linkProperties = allProperties
                .Where(p => p.CanWrite && p.PropertyType == typeof(Link));

            foreach (var linkProperty in linkProperties)
            {
                if (linkProperty.GetValue(model) is not Link link) continue;
                var rewritten = rewriter.Rewrite(link);
                if (rewritten == null) continue;

                linkProperty.SetValue(model, rewritten);

                // Special handling of the hidden Self property:
                // unwrap into the root object
                if (linkProperty.Name == nameof(Resource.Self))
                {
                    // Turn these on when Resource inherits Link
                    //allProperties
                    //    .SingleOrDefault(p => p.Name == nameof(Resource.Href))
                    //    ?.SetValue(model, rewritten.Href);

                    //allProperties
                    //    .SingleOrDefault(p => p.Name == nameof(Resource.Method))
                    //    ?.SetValue(model, rewritten.Method);

                    //allProperties
                    //    .SingleOrDefault(p => p.Name == nameof(Resource.Relations))
                    //    ?.SetValue(model, rewritten.Relations);
                }
            }

            var arrayProperties = allProperties.Where(p => p.PropertyType.IsArray);
            RewriteLinksInArrays(arrayProperties, model, rewriter);

            var objectProperties = allProperties
                .Except(linkProperties)
                .Except(arrayProperties);
            RewriteLinksInNestedObjects(objectProperties, model, rewriter);
        }

        private static void RewriteLinksInNestedObjects(
            IEnumerable<PropertyInfo> objectProperties,
            object model,
            LinkRewriter rewriter)
        {
            foreach (var objectProperty in objectProperties)
            {
                if (objectProperty.PropertyType == typeof(string))
                {
                    continue;
                }

                var typeInfo = objectProperty.PropertyType.GetTypeInfo();
                if (typeInfo.IsClass)
                {
                    var val = objectProperty.GetValue(model);
                    if (val == null) continue;
                    RewriteAllLinks(val, rewriter);
                }
            }
        }

        private static void RewriteLinksInArrays(
            IEnumerable<PropertyInfo> arrayProperties,
            object model,
            LinkRewriter rewriter)
        {

            foreach (var arrayProperty in arrayProperties)
            {
                var array = arrayProperty.GetValue(model) as Array ?? Array.Empty<Array>();

                foreach (var element in array)
                {
                    RewriteAllLinks(element, rewriter);
                }
            }
        }

    }
}
