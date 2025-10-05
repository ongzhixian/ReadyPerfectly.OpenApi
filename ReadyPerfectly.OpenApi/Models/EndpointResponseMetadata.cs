using Microsoft.OpenApi.Models;

namespace ReadyPerfectly.OpenApi.Models;

public class EndpointResponseMetadata
{
    public string Name { get; internal set; }

    public Dictionary<string, OpenApiMediaType> Content { get; internal set; }
}