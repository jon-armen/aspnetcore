using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Json.Schema;
using Json.Schema.Generation;

/// <summary>
/// Manages resolving an OpenAPI schema for given types and maintaining the schema cache.
/// </summary>
public class OpenApiComponentService(JsonSchemaBuilder schemaBuilder)
{
    private readonly Dictionary<Type, OpenApiSchema> _typeToOpenApiSchema = new();
    // private readonly JsonSerializerOptions _serializerOptions = jsonOptions.Value.SerializerOptions;
    // private readonly DefaultJsonTypeInfoResolver _defaultJsonTypeInfoResolver = new();

    internal OpenApiComponents GetOpenApiComponents()
    {
        var components = new OpenApiComponents();
        foreach (var (type, schema) in _typeToOpenApiSchema)
        {
            components.Schemas.Add(GetReferenceId(type), schema);
        }
        return components;
    }

    internal OpenApiSchema GetOrCreateOpenApiSchemaForType(Type type)
    {
        if (_typeToOpenApiSchema.TryGetValue(type, out var cachedSchema))
        {
            return cachedSchema;
        }
        var schema = schemaBuilder.FromType(type).Build();
    }

    /*
    internal OpenApiSchema GetOrCreateOpenApiSchemaForType(Type type, ApiParameterDescription? parameterDescription = null)
    {
        if (_typeToOpenApiSchema.TryGetValue(type, out var cachedSchema))
        {
            return cachedSchema;
        }
        var jsonType = _defaultJsonTypeInfoResolver.GetTypeInfo(type, _serializerOptions);
        var schema = new OpenApiSchema();
        var useRef = false;
        var addToCache = false;
        if (jsonType.Type == typeof(JsonNode))
        {
            schema.Type = "object";
            schema.AdditionalPropertiesAllowed = true;
            schema.AdditionalProperties = new OpenApiSchema
            {
                Type = "object"
            };
            return schema;
        }
        if (jsonType.Kind == JsonTypeInfoKind.Dictionary)
        {
            schema.Type = "object";
            schema.AdditionalPropertiesAllowed = true;
            var genericTypeArgs = jsonType.Type.GetGenericArguments();
            Type? valueType = null;
            if (genericTypeArgs.Length == 2)
            {
                valueType = jsonType.Type.GetGenericArguments().Last();
            }
            schema.AdditionalProperties = OpenApiTypeMapper.MapTypeToOpenApiPrimitiveType(valueType);
        }
        if (jsonType.Kind == JsonTypeInfoKind.None)
        {
            if (type.IsEnum)
            {
                schema = OpenApiTypeMapper.MapTypeToOpenApiPrimitiveType(type.GetEnumUnderlyingType());
                foreach (var value in Enum.GetValues(type))
                {
                    schema.Enum.Add(new OpenApiInteger((int)value));
                }
            }
            else
            {
                schema = OpenApiTypeMapper.MapTypeToOpenApiPrimitiveType(type);
                var defaultValueAttribute = jsonType.Type.GetCustomAttributes(true).OfType<DefaultValueAttribute>().FirstOrDefault();
                if (defaultValueAttribute != null)
                {
                    schema.Default = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(defaultValueAttribute.Value));
                }
                if (parameterDescription is not null && parameterDescription.DefaultValue is not null)
                {
                    schema.Default = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(parameterDescription.DefaultValue));
                }
            }
        }
        if (jsonType.Kind == JsonTypeInfoKind.Enumerable)
        {
            schema.Type = "array";
            var elementType = jsonType.Type.GetElementType() ?? jsonType.Type.GetGenericArguments().First();
            schema.Items = OpenApiTypeMapper.MapTypeToOpenApiPrimitiveType(elementType);
        }
        if (jsonType.Kind == JsonTypeInfoKind.Object)
        {
            if (jsonType.PolymorphismOptions is {} polymorphismOptions && polymorphismOptions.DerivedTypes.Count > 0)
            {
                foreach (var derivedType in polymorphismOptions.DerivedTypes)
                {
                    schema.OneOf.Add(GetOrCreateOpenApiSchemaForType(derivedType.DerivedType));
                }
            }
            if (jsonType.Type.BaseType is { } baseType && baseType != typeof(object))
            {
                schema.AllOf.Add(GetOrCreateOpenApiSchemaForType(baseType));
            }
            addToCache = true;
            useRef = true;
            schema.Type = "object";
            schema.AdditionalPropertiesAllowed = false;
            foreach (var property in jsonType.Properties)
            {
                if (jsonType.Type.GetProperty(property.Name) is { } propertyInfo && propertyInfo.DeclaringType != jsonType.Type)
                {
                    continue;
                }
                var innerSchema = GetOrCreateOpenApiSchemaForType(property.PropertyType);
                var defaultValueAttribute = property.AttributeProvider!.GetCustomAttributes(true).OfType<DefaultValueAttribute>().FirstOrDefault();
                if (defaultValueAttribute != null)
                {
                    innerSchema.Default = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(defaultValueAttribute.Value));
                }
                innerSchema.ReadOnly = property.Set is null;
                innerSchema.WriteOnly = property.Get is null;
                schema.Properties.Add(property.Name, innerSchema);
            }
        }
        if (addToCache)
        {
            _typeToOpenApiSchema[type] = schema;
        }
        if (useRef)
        {
            schema.Reference = new OpenApiReference { Id = GetReferenceId(type), Type = ReferenceType.Schema };
        }
        return schema;
    }
    */

    private string GetReferenceId(Type type)
    {
        if (!type.IsConstructedGenericType)
        {
            return type.Name.Replace("[]", "Array");
        }

        var prefix = type.GetGenericArguments()
            .Select(GetReferenceId)
            .Aggregate((previous, current) => previous + current);

        if (IsAnonymousType(type))
        {
            return prefix + "AnonymousType";
        }

        return prefix + type.Name.Split('`').First();
    }

    private static bool IsAnonymousType(Type type) => type.GetTypeInfo().IsClass
        && type.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute))
        && !type.IsNested
        && type.Name.StartsWith("<>", StringComparison.Ordinal)
        && type.Name.Contains("__Anonymous");
}
