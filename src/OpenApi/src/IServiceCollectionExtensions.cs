using Microsoft.Extensions.DependencyInjection;
using Json.Schema;

/// <summary>
/// OpenAPI-related methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSingleton<JsonSchemaBuilder>();
        services.AddSingleton<OpenApiComponentService>();
        services.AddTransient<OpenApiDocumentService>();
        return services;
    }
}
