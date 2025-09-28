using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace ReadyPerfectly.OpenApi.Extensions;

public static class IConfigurationExtensions
{
    public static OpenApiInfo GetOpenApiInfoFromSection(
        this IConfiguration configuration, 
        string configurationSectionName)
    {
        var openApiInfo = new OpenApiInfo();
        configuration.GetSection(configurationSectionName).Bind(openApiInfo);
        return openApiInfo;
    }
}
