public class OpenApiDocumentServiceTest : OpenApiDocumentServiceTestBase
{
    [Fact]
    public async Task GenerateOpenApiDocument_WithRouteParameter()
        => await VerifyOpenApiDocument("/test-with-route-params/{id}/{name}/{age}", (Guid id, string name, int age) => { });

    [Fact]
    public async Task GenerateOpenApiDocument_WithRouteParameter_Nullable()
        => await VerifyOpenApiDocument("/test-with-route-params/{id}/{name}/{age}", (Guid? id, string name, int? age) => { });

    [Fact]
    public async Task GenerateOpenApiDocument_WithRouteParameter_DefaultValues()
        => await VerifyOpenApiDocument("/test-with-route-params/{id}/{name}/{age}", (Guid id, string name = "default-name", int age = 42) => { });

    [Fact]
    public async Task GenerateOpenApiDocument_WithQueryParameter()
        => await VerifyOpenApiDocument("/test-with-query-parameters", (Guid id, string name, int age) => { });

    [Fact]
    public async Task GenerateOpenApiDocument_WithQueryParameter_Nullable()
        => await VerifyOpenApiDocument("/test-with-query-params", (Guid? id, string name, int? age) => { });

    [Fact]
    public async Task GenerateOpenApiDocument_WithQueryParameter_DefaultValues()
        => await VerifyOpenApiDocument("/test-with-query-params", (Guid id, string name = "default-name", int age = 42) => { });

    [Fact]
    public async Task GenerateOpenApiDocument_WithBodyParam()
        => await VerifyOpenApiDocument("/test-with-body-params", (Todo todo) => { }, ["POST"]);

    [Fact]
    public async Task GenerateOpenApiDocument_WithBodyParam_Inheritance()
        => await VerifyOpenApiDocument("/test-with-body-params", (TodoWithDueDate todo) => { }, ["POST"]);

    [Fact]
    public async Task GenerateOpenApiDocument_WithBodyParam_Inheritance_Interface()
        => await VerifyOpenApiDocument("/test-with-body-params", (TodoFromInterface todo) => { }, ["POST"]);

    [Fact]
    public async Task GenerateOpenApiDocument_WithBodyParam_WithPolymorphismAttributes()
        => await VerifyOpenApiDocument("/test-with-body-params", (Shape todo) => { }, ["POST"]);

    [Fact]
    public async Task GenerateOpenApiDocument_WithEnumParameter()
        => await VerifyOpenApiDocument("/test-with-enum-param", (TodoStatus todo) => { });

    [Fact]
    public async Task GenerateOpenApiDocument_WithComplexResponse()
        => await VerifyOpenApiDocument("/test-with-complex-response", () => new Todo(1, "Test title", false, DateTime.Now));

    [Fact]
    public async Task GenerateOpenApiDocument_WithAnonymousResponse()
        => await VerifyOpenApiDocument("/test-with-anon-response", () => new { Id = 1, Title = "Test title" });
}
