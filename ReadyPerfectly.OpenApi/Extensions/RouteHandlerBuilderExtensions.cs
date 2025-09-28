using System.Text;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace ReadyPerfectly.OpenApi.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static void OpenApiDocumentation(this RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder.Add(endpointBuilder =>
        {
            //System.Diagnostics.Debugger.Break();

            var operationId = DeriveOperationId(endpointBuilder);

            // WithName(operationId) equivalent
            endpointBuilder.AddEndpointNameMetadata(operationId);
            endpointBuilder.AddRouteNameMetadata(operationId);

            endpointBuilder.AddEndpointSummary("Placeholder for endpointSummary");
            endpointBuilder.AddEndpointDescription("Placeholder for endpointDescription");
            endpointBuilder.AddTags("Tag 1");

            //System.Diagnostics.Debugger.Break();
        });
    }

    public static void OpenApiDocumentation(this RouteHandlerBuilder routeHandlerBuilder, string operationId)
    {
        routeHandlerBuilder.Add(endpointBuilder =>
        {
            // WithName(operationId) equivalent
            endpointBuilder.AddEndpointNameMetadata(operationId);
            endpointBuilder.AddRouteNameMetadata(operationId);
        });
    }


    public static void WithOpenApiOperation(this RouteHandlerBuilder routeHandlerBuilder,
        string operationId
        )
    {
        //routeHandlerBuilder
        routeHandlerBuilder.WithName(operationId);
        routeHandlerBuilder.WithSummary("Summary");
        routeHandlerBuilder.WithDescription("Some Description");
        routeHandlerBuilder.WithTags("DateTime", "AA");
        //operation.Tags = [new() { Name = "DateTime" }];

        routeHandlerBuilder.Add(endpointBuilder =>
        {
            System.Diagnostics.Debugger.Break();
        });
    }



    //public static OpenApiInfo GetOpenApiInfoFromSection(
    //    this IConfiguration configuration,
    //    string configurationSectionName)
    //{
    //    var openApiInfo = new OpenApiInfo();
    //    configuration.GetSection(configurationSectionName).Bind(openApiInfo);
    //    return openApiInfo;
    //}

    // EndpointNameMetadata

    private static void AddEndpointNameMetadata(this EndpointBuilder endpointBuilder, string operationId)
    {
        if (!endpointBuilder.Metadata.HasEndpointNameMetadata(operationId))
            endpointBuilder.Metadata.Add(new EndpointNameMetadata(operationId));
    }

    private static bool HasEndpointNameMetadata(this IList<object> objectList, string endpointName)
    {
        return objectList.Any(instance => instance is EndpointNameMetadata metaData
            && string.Equals(metaData.EndpointName, endpointName, StringComparison.InvariantCultureIgnoreCase));
    }

    // RouteNameMetadata

    private static void AddRouteNameMetadata(this EndpointBuilder endpointBuilder, string operationId)
    {
        if (!endpointBuilder.Metadata.HasRouteNameMetadata(operationId))
            endpointBuilder.Metadata.Add(new RouteNameMetadata(operationId));
    }

    private static bool HasRouteNameMetadata(this IList<object> objectList, string routeName)
    {
        return objectList.Any(instance => instance is RouteNameMetadata metaData
            && string.Equals(metaData.RouteName, routeName, StringComparison.InvariantCultureIgnoreCase));
    }

    // AddEndpointSummary

    private static void AddEndpointSummary(this EndpointBuilder endpointBuilder, string summaryMessage)
    {
        if (!endpointBuilder.Metadata.HasEndpointSummaryAttribute())
            endpointBuilder.Metadata.Add(new EndpointSummaryAttribute(summaryMessage));
    }

    private static bool HasEndpointSummaryAttribute(this IList<object> objectList)
    {
        return objectList.Any(instance => instance is EndpointSummaryAttribute);
    }

    // AddEndpointDescription

    private static void AddEndpointDescription(this EndpointBuilder endpointBuilder, string description)
    {
        if (!endpointBuilder.Metadata.HasEndpointDescriptionAttribute())
            endpointBuilder.Metadata.Add(new EndpointDescriptionAttribute(description));
    }

    private static bool HasEndpointDescriptionAttribute(this IList<object> objectList)
    {
        return objectList.Any(instance => instance is EndpointDescriptionAttribute);
    }

    // AddTags

    private static void AddTags(this EndpointBuilder endpointBuilder, params string[] tags)
    {
        if (!endpointBuilder.Metadata.HasTagsAttribute())
            endpointBuilder.Metadata.Add(new TagsAttribute(tags));
    }

    private static bool HasTagsAttribute(this IList<object> objectList)
    {
        return objectList.Any(instance => instance is TagsAttribute);
    }

    // DeriveOperationId

    private static string DeriveOperationId(EndpointBuilder endpointBuilder)
    {
        if (endpointBuilder is not RouteEndpointBuilder routeEndpointBuilder)
            return string.Empty;

        var sb = new StringBuilder();

        var httpMethodMetadata = endpointBuilder.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();

        if (httpMethodMetadata?.HttpMethods.Count > 0)
            sb.Append(httpMethodMetadata.HttpMethods[0].ToLowerInvariant());

        var needsSeparator = sb.Length > 0;

        foreach (var segment in routeEndpointBuilder.RoutePattern.PathSegments)
            foreach (var part in segment.Parts)
                if (part is RoutePatternLiteralPart literal && !literal.IsParameter && !literal.IsSeparator)
                {
                    if (needsSeparator)
                        sb.Append('-');

                    sb.Append(literal.Content);
                    needsSeparator = true;
                }

        return sb.ToString();
    }


    private static string NormalizeRouteTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        string normalized = template.Replace("/", "-");

        // Remove route parameter braces, type constraints, and optional markers
        normalized = Regex.Replace(normalized, @"\{([^}:?]+)(:[^}]+)?\??\}", "$1");

        return normalized;
    }


    // Note: RouteHandlerBuilder : IEndpointConventionBuilder
    // So if want a more generic, we should opt to extend IEndpointConventionBuilder instead
    // Shelved for now.
    //public static void WithOpenApiOperation(this IEndpointConventionBuilder routeHandlerBuilder)
    //{
    //    //routeHandlerBuilder
    //    System.Diagnostics.Debugger.Break();
    //}
}