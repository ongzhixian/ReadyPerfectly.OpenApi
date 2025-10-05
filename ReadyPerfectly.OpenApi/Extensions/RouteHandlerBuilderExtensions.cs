using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using ReadyPerfectly.OpenApi.Models;

namespace ReadyPerfectly.OpenApi.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static void DebugOpenApiDocumentation(this RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder.Add(endpointBuilder =>
        {
            System.Diagnostics.Debugger.Break();

            //var operationId = DeriveOperationId(endpointBuilder);

            //// WithName(operationId) equivalent
            //endpointBuilder.AddEndpointNameMetadata(operationId);
            //endpointBuilder.AddRouteNameMetadata(operationId);

            //endpointBuilder.AddEndpointSummary("Placeholder for endpointSummary");
            //endpointBuilder.AddEndpointDescription("Placeholder for endpointDescription");
            //endpointBuilder.AddTags("Tag 1");

            //endpointBuilder.Metadata[3].GetType().ToString()
            //"Microsoft.AspNetCore.Http.ProducesResponseTypeMetadata"


            //System.Diagnostics.Debugger.Break();
            //var produce = new Microsoft.AspNetCore.Http.ProducesResponseTypeMetadata(
            //    200, typeof(DateTime), ["application/json"]);
            //endpointBuilder.Metadata.Add(produce);
        });

        routeHandlerBuilder.Finally(endpointBuilder =>
        {
            System.Diagnostics.Debugger.Break();
        });
    }

    // FOLLOW MICROSOFT OPENAPI STYLE

    /// <summary>
    /// Adds OpenApiOperation
    /// </summary>
    /// <param name="routeHandlerBuilder"></param>
    /// <returns></returns>
    /// <remarks>This supplements the Microsoft's metadata attributes.</remarks>
    public static RouteHandlerBuilder WithOperation(this RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder.Add(endpointBuilder =>
        {
            //endpointBuilder.Metadata.First<System.Reflection.RuntimeMethodInfo>


            // Without applying any of metadata attributes
            // endpointBuilder.Metadata
            // Count = 4
            //     [0]: {Microsoft.AspNetCore.Http.IResult GetDateTimeForTimeZone(System.String)}
            //     [1]: HttpMethods: GET, Cors: False
            //     [2]: {Microsoft.AspNetCore.Http.Metadata.ParameterBindingMetadata}
            //     [3]: {Microsoft.OpenApi.Models.OpenApiOperation}
            // Where OpenApiOperation looks like:
            // endpointBuilder.Metadata[3]
            // {Microsoft.OpenApi.Models.OpenApiOperation}
            //     Annotations: null
            //     Callbacks: Count = 0
            //     Deprecated: false
            //     Description: null
            //     Extensions: Count = 0
            //     ExternalDocs: null
            //     OperationId: null
            //     Parameters: Count = 1
            //     RequestBody: null
            //     Responses: Count = 1
            //    Security: Count = 0
            //     Servers: Count = 0
            //     Summary: null
            //     Tags: Count = 1


            // Construct the minimal OpenApiOperation

            Microsoft.OpenApi.Models.OpenApiOperation openApiOperation = new Microsoft.OpenApi.Models.OpenApiOperation();

            // Add OperationId

            //        EndpointName: GetDateTimeForTimeZone
            //EndpointName: "GetDateTimeForTimeZone"
            //endpointBuilder.Metadata[4]
            //RouteName: GetDateTimeForTimeZone
            //    RouteName: "GetDateTimeForTimeZone"

            //            endpointBuilder.Metadata[4].GetType().ToString()
            //"Microsoft.AspNetCore.Routing.RouteNameMetadata"
            //endpointBuilder.Metadata[3].GetType().ToString()
            //"Microsoft.AspNetCore.Routing.EndpointNameMetadata"

            // While RouteNameMetadata and EndpointNameMetadata are
            //Microsoft.AspNetCore.Routing.RouteNameMetadata
            //Microsoft.AspNetCore.Routing.EndpointNameMetadata

            var endpointName = (Microsoft.AspNetCore.Routing.EndpointNameMetadata?)endpointBuilder
                .Metadata.FirstOrDefault(r => r is Microsoft.AspNetCore.Routing.EndpointNameMetadata);
            if (endpointName != null)
            {
                openApiOperation.OperationId = endpointName.EndpointName; // Do we need to add some normalization mechanism here?
            }


            // Add Summary 

            //            endpointBuilder.Metadata[3]
            //Summary: Gets date time
            //    Summary: "Gets date time"
            //    TypeId: { Name = "EndpointSummaryAttribute" FullName = "Microsoft.AspNetCore.Http.EndpointSummaryAttribute"}

            var summaryAttribute = (Microsoft.AspNetCore.Http.EndpointSummaryAttribute?)endpointBuilder
                .Metadata.FirstOrDefault(r => r is Microsoft.AspNetCore.Http.EndpointSummaryAttribute);
            if (summaryAttribute != null)
            {
                openApiOperation.Summary = summaryAttribute.Summary;
            }


            // Add Description

            //            endpointBuilder.Metadata[4]
            //Description: Returns the datetime of server
            //    Description: "Returns the datetime of server"
            //    TypeId: { Name = "EndpointDescriptionAttribute" FullName = "Microsoft.AspNetCore.Http.EndpointDescriptionAttribute"}

            var descriptionAttribute = (Microsoft.AspNetCore.Http.EndpointDescriptionAttribute?)endpointBuilder
                .Metadata.FirstOrDefault(r => r is Microsoft.AspNetCore.Http.EndpointDescriptionAttribute);
            if (descriptionAttribute != null)
            {
                openApiOperation.Description = descriptionAttribute.Description;
            }


            // Tasks:
            // 1. Parameters: Count = 1
            // 2. Responses: Count = 1
            // 3. Tags: Count = 1

            // Deault Tag is ClarusServiceApi

            // We assume there is only one System.Reflection.MethodInfo.
            // Aside: At runtime reflection, the exact type is really: System.Reflection.RuntimeMethodInfo.
            //        But this is an internal class. So we go up the inheritence hierachy.

            var runtimeMethodInfo = (System.Reflection.MethodInfo?)endpointBuilder.Metadata
                .FirstOrDefault(q => q is System.Reflection.MethodInfo);

            // Responses and Tags derived from MethodInfo
            if (runtimeMethodInfo is System.Reflection.MethodInfo methodInfo)
            {
                // TODO: Add Response


                if (typeof(IResult).IsAssignableFrom(methodInfo.ReturnType))
                {
                    var key = ((int)HttpStatusCode.OK).ToString();

                    // TODO: Needs work; we need to figure out some more where "200" comes from
                    //       For now, go with adding a default OK response.
                    openApiOperation.Responses.Add(key, new Microsoft.OpenApi.Models.OpenApiResponse
                    {
                        Description = HttpStatusCode.OK.ToString(),
                    });

                    var responseMeta = (ReadyPerfectly.OpenApi.Models.EndpointResponseMetadata?)
                        endpointBuilder.Metadata.FirstOrDefault(r => r is ReadyPerfectly.OpenApi.Models.EndpointResponseMetadata data
                        && data.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

                    if (responseMeta != null) 
                    {
                        openApiOperation.Responses[key].Content = responseMeta.Content;
                    }
                }

                // TODO: Add Tag
                // 

                //endpointBuilder.Metadata[3].GetType().ToString()
                //"Microsoft.AspNetCore.Http.TagsAttribute"

                var tagAttributeList = endpointBuilder.Metadata.Where(r => r is Microsoft.AspNetCore.Http.TagsAttribute)
                    .Cast<Microsoft.AspNetCore.Http.TagsAttribute>();

                if (tagAttributeList.Any())
                {
                    foreach (var tagAttribute in tagAttributeList)
                    {
                        foreach (var item in tagAttribute.Tags)
                        {
                            openApiOperation.Tags.Add(new Microsoft.OpenApi.Models.OpenApiTag
                            {
                                Name = item
                            });
                        }
                    }
                }
                else
                {
                    // Add a default tag name

                    if (methodInfo.ReflectedType?.Name is string reflectedTypeName)
                    {
                        openApiOperation.Tags.Add(new Microsoft.OpenApi.Models.OpenApiTag
                        {
                            Name = reflectedTypeName,
                        });
                    }
                }
            }

            // TODO: Add Parameters
            //Microsoft.AspNetCore.Http.Metadata.ParameterBindingMetadata

            var parameterBindingMetadataList = endpointBuilder.Metadata
                .Where(obj => obj is Microsoft.AspNetCore.Http.Metadata.IParameterBindingMetadata)
                .Cast<Microsoft.AspNetCore.Http.Metadata.IParameterBindingMetadata>();

            foreach (var item in parameterBindingMetadataList)
            {
                System.Diagnostics.Debugger.Break();

                
                var param = new Microsoft.OpenApi.Models.OpenApiParameter
                {
                    In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                    Name = item.Name,
                    Required = !item.IsOptional,
                    Style = Microsoft.OpenApi.Models.ParameterStyle.Form
                };

                var parameterMeta = (ReadyPerfectly.OpenApi.Models.EndpointParameterMetadata?)endpointBuilder.Metadata.FirstOrDefault(r => r is ReadyPerfectly.OpenApi.Models.EndpointParameterMetadata s
                    && s.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase));

                if (parameterMeta != null)
                {
                    param.Schema = parameterMeta.Schema;
                    param.Examples = parameterMeta.Examples;
                    param.Example = parameterMeta.Example;
                }

                openApiOperation.Parameters.Add(param);
            }


            //var operationId = DeriveOperationId(endpointBuilder);

            //// WithName(operationId) equivalent
            //endpointBuilder.AddEndpointNameMetadata(operationId);
            //endpointBuilder.AddRouteNameMetadata(operationId);

            //endpointBuilder.AddEndpointSummary("Placeholder for endpointSummary");
            //endpointBuilder.AddEndpointDescription("Placeholder for endpointDescription");
            //endpointBuilder.AddTags("Tag 1");

            //endpointBuilder.Metadata[3].GetType().ToString()
            //"Microsoft.AspNetCore.Http.ProducesResponseTypeMetadata"


            //System.Diagnostics.Debugger.Break();
            //var produce = new Microsoft.AspNetCore.Http.ProducesResponseTypeMetadata(
            //    200, typeof(DateTime), ["application/json"]);
            //endpointBuilder.Metadata.Add(produce);


            endpointBuilder.Metadata.Add(openApiOperation); // Should add only if not exists
        });

        routeHandlerBuilder.Finally(endpointBuilder =>
        {
            System.Diagnostics.Debugger.Break();
        });

        return routeHandlerBuilder;
    }

    // WithOperationParameter

    public static RouteHandlerBuilder WithOperationParameter(this RouteHandlerBuilder routeHandlerBuilder,
        string parameterName, OpenApiSchema? openApiSchema = null, IDictionary<string, OpenApiExample>? examples = null, IOpenApiAny? example = null)
    {

        //    if (openApiOperation.Parameters.FirstOrDefault(r => r.Name.Equals("timeZoneInfoId", StringComparison.InvariantCultureIgnoreCase))
        //is OpenApiParameter parameter)
        //    {
        //        // For server
        //        parameter.Schema = new OpenApiSchema
        //        {
        //            Type = "string",
        //            //Format = "string", 
        //            //Default = new OpenApiString("asd"),
        //            //Example = new OpenApiString("UTC")
        //        };

        //        parameter.Examples = TimeZoneInfo.GetSystemTimeZones().ToDictionary(r => r.Id, r => new OpenApiExample
        //        {
        //            Summary = $"{r.BaseUtcOffset} {r.StandardName}",
        //            Value = new OpenApiString(r.Id)
        //        });

        //        parameter.Example = parameter.Examples["Singapore Standard Time"].Value;
        //    }

        routeHandlerBuilder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(new EndpointParameterMetadata
            {
                Name = parameterName,
                Schema = openApiSchema,
                Examples = examples,
                Example = example
            });
        });

        return routeHandlerBuilder;
    }

    // WithOperationResponse

    public static RouteHandlerBuilder WithOperationResponse(this RouteHandlerBuilder routeHandlerBuilder,
        string statusCode, Dictionary<string, OpenApiMediaType> responseContent)
    {
        routeHandlerBuilder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(new EndpointResponseMetadata
            {
                Name = statusCode,
                Content = responseContent
            });
        });

        return routeHandlerBuilder;
    }



    // IDEA: CONSOLIDATED

    public static void OpenApiDocumentation(this RouteHandlerBuilder routeHandlerBuilder, Action<EndpointBuilder> endpointBuilderAction)
    {
        routeHandlerBuilder.Add(endpointBuilderAction);
    }

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