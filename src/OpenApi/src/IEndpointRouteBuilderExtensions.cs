using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Writers;

/// <summary>
///
/// </summary>
public static class IEndpointRouteBuilderExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder MapOpenApiDocument(this IEndpointRouteBuilder endpoints, string pattern = "/openapi.json") =>
        endpoints.MapGet(pattern, async (OpenApiDocumentService openApiDocumentService, HttpContext context) =>
            {
                try
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json;charset=utf-8";
                    using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
                    var jsonWriter = new OpenApiJsonWriter(textWriter);
                    openApiDocumentService.Document.SerializeAsV3(jsonWriter);
                    await context.Response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { message = ex.Message });
                }
            }).ExcludeFromDescription();
}
