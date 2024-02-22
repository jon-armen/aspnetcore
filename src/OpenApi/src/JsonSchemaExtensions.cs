using System.Linq;
using Json.Schema;
using Json.Schema.OpenApi;
using Microsoft.OpenApi.Models;

internal static class JsonSchemaExtensions
{
    internal static OpenApiSchema ToOpenApiSchema(this JsonSchema schema)
    {
        var openApiSchema = new OpenApiSchema()
        {
            Discriminator = schema.GetDiscriminator(),
            Maximum = schema.GetMaximum(),
            Minimum = schema.GetMinimum(),
            MinItems = checked((int?)schema.GetMinItems()),
            MinLength = checked((int?)schema.GetMinLength()),
            Pattern = schema.GetPattern()?.ToString(),
            Properties = schema.GetProperties()?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToOpenApiSchema()),
            ReadOnly = schema.GetReadOnly() ?? false,
            Required = schema.GetRequired()?.ToHashSet(),
            Type = schema.GetJsonType().ToOpenApiSchemaType(),
            UniqueItems = schema.GetUniqueItems() ?? false,
            WriteOnly = schema.GetWriteOnly() ?? false,
        };
        return openApiSchema;
    }

    internal static string ToOpenApiSchemaType(this SchemaValueType? jsonType)
    {
        return jsonType switch
        {
            SchemaValueType.Array => "array",
            SchemaValueType.Boolean => "boolean",
            SchemaValueType.Integer => "integer",
            SchemaValueType.Number => "number",
            SchemaValueType.Object => "object",
            SchemaValueType.String => "string",
            SchemaValueType.Null => "null",
            _ => throw new ArgumentOutOfRangeException(nameof(jsonType), jsonType, null),
        };
    }
}
