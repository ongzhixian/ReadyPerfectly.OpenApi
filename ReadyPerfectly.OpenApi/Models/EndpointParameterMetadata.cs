using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace ReadyPerfectly.OpenApi.Models;

public class EndpointParameterMetadata
{
    public string Name { get; set; }

    public OpenApiSchema? Schema { get; set; }
    
    public IOpenApiAny? Example { get; set; }
    
    public IDictionary<string, OpenApiExample>? Examples { get; set; }
}
