//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在REF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace TouchSocket.Mcp;

/// <summary>
/// 提供 MCP JSON Schema 生成能力。
/// </summary>
public static class McpJsonSchemaGenerator
{
    #region Methods

    /// <summary>
    /// 根据指定类型生成 JSON Schema。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="jsonSerializerOptions">JSON 序列化选项。</param>
    /// <returns>JSON Schema。</returns>
    public static McpToolProperty Generate<T>(JsonSerializerOptions jsonSerializerOptions = null, bool requireValueTypes = true)
    {
        return Generate(typeof(T), null, jsonSerializerOptions, requireValueTypes);
    }

    /// <summary>
    /// 根据指定类型生成 JSON Schema。
    /// </summary>
    /// <param name="type">目标类型。</param>
    /// <param name="description">Schema 描述。</param>
    /// <param name="jsonSerializerOptions">JSON 序列化选项。</param>
    /// <returns>JSON Schema。</returns>
    public static McpToolProperty Generate(Type type, string description = null, JsonSerializerOptions jsonSerializerOptions = null, bool requireValueTypes = true)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var options = jsonSerializerOptions ?? McpOptionsBase.CreateDefaultJsonSerializerOptions();
        return Generate(options.GetTypeInfo(type), description, options, requireValueTypes, new HashSet<Type>());
    }

    /// <summary>
    /// 根据指定类型生成 JSON Schema 字符串。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="jsonSerializerOptions">JSON 序列化选项。</param>
    /// <returns>JSON Schema 字符串。</returns>
    public static string GenerateJson<T>(JsonSerializerOptions jsonSerializerOptions = null, bool requireValueTypes = true)
    {
        return GenerateJson(typeof(T), null, jsonSerializerOptions, requireValueTypes);
    }

    /// <summary>
    /// 根据指定类型生成 JSON Schema 字符串。
    /// </summary>
    /// <param name="type">目标类型。</param>
    /// <param name="description">Schema 描述。</param>
    /// <param name="jsonSerializerOptions">JSON 序列化选项。</param>
    /// <returns>JSON Schema 字符串。</returns>
    public static string GenerateJson(Type type, string description = null, JsonSerializerOptions jsonSerializerOptions = null, bool requireValueTypes = true)
    {
        var options = new JsonSerializerOptions(jsonSerializerOptions ?? McpOptionsBase.CreateDefaultJsonSerializerOptions());
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        return JsonSerializer.Serialize(Generate(type, description, options, requireValueTypes), options.GetTypeInfo(typeof(McpToolProperty)));
    }

    internal static McpToolProperty GenerateForTool(Type type, string description = null, JsonSerializerOptions jsonSerializerOptions = null)
    {
        return Generate(type, description, jsonSerializerOptions, true);
    }

    private static McpToolProperty Generate(JsonTypeInfo jsonTypeInfo, string description, JsonSerializerOptions jsonSerializerOptions, bool requireValueTypes, HashSet<Type> visitingTypes)
    {
        var type = jsonTypeInfo.Type;
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        var schema = new McpToolProperty
        {
            Description = description ?? underlying.GetCustomAttribute<DescriptionAttribute>()?.Description
        };

        if (underlying == typeof(string)
            || underlying == typeof(char)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateTimeOffset)
            || underlying == typeof(Guid)
            || underlying == typeof(TimeSpan)
            || underlying == typeof(Uri))
        {
            schema.Type = "string";
            return schema;
        }

        if (underlying == typeof(bool))
        {
            schema.Type = "boolean";
            return schema;
        }

        if (underlying == typeof(int) || underlying == typeof(long)
            || underlying == typeof(short) || underlying == typeof(byte)
            || underlying == typeof(uint) || underlying == typeof(ulong)
            || underlying == typeof(ushort) || underlying == typeof(sbyte))
        {
            schema.Type = "integer";
            return schema;
        }

        if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
        {
            schema.Type = "number";
            return schema;
        }

        if (underlying.IsEnum)
        {
            schema.Type = "string";
            schema.Enum = Enum.GetNames(underlying);
            return schema;
        }

        if (TryGetEnumerableElementType(underlying, out var elementType))
        {
            schema.Type = "array";
            schema.Items = Generate(jsonSerializerOptions.GetTypeInfo(elementType), null, jsonSerializerOptions, requireValueTypes, visitingTypes);
            return schema;
        }

        schema.Type = "object";
        if (!visitingTypes.Add(underlying))
        {
            return schema;
        }

        try
        {
            var properties = new Dictionary<string, McpToolProperty>();
            var required = new List<string>();

            foreach (var propertyInfo in jsonTypeInfo.Properties)
            {
                properties[propertyInfo.Name] = Generate(
                    jsonSerializerOptions.GetTypeInfo(propertyInfo.PropertyType),
                    GetDescription(jsonTypeInfo.Type, propertyInfo, jsonSerializerOptions),
                    jsonSerializerOptions,
                    requireValueTypes,
                    visitingTypes);

                if (IsRequired(propertyInfo, requireValueTypes))
                {
                    required.Add(propertyInfo.Name);
                }
            }

            if (properties.Count > 0)
            {
                schema.Properties = properties;
            }

            if (required.Count > 0)
            {
                schema.Required = required;
            }

            return schema;
        }
        finally
        {
            visitingTypes.Remove(underlying);
        }
    }

    private static string GetDescription(Type type, JsonPropertyInfo jsonPropertyInfo, JsonSerializerOptions jsonSerializerOptions)
    {
        var description = GetDescription(jsonPropertyInfo.AttributeProvider);
        if (!string.IsNullOrEmpty(description))
        {
            return description;
        }

        foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (propertyInfo.GetMethod is null || propertyInfo.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (jsonPropertyInfo.Name == GetJsonPropertyName(propertyInfo, jsonSerializerOptions))
            {
                return GetDescription(propertyInfo);
            }
        }

        return null;
    }

    private static string GetDescription(ICustomAttributeProvider attributeProvider)
    {
        if (attributeProvider is null)
        {
            return null;
        }

        var attributes = attributeProvider.GetCustomAttributes(typeof(DescriptionAttribute), true);
        return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : null;
    }

    private static string GetJsonPropertyName(PropertyInfo propertyInfo, JsonSerializerOptions jsonSerializerOptions)
    {
        var jsonPropertyName = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
        if (!string.IsNullOrEmpty(jsonPropertyName))
        {
            return jsonPropertyName;
        }

        return jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
    }

    private static bool IsRequired(JsonPropertyInfo propertyInfo, bool requireValueTypes)
    {
        if (!requireValueTypes)
        {
            return false;
        }

        return propertyInfo.IsRequired
            || (propertyInfo.PropertyType.IsValueType && Nullable.GetUnderlyingType(propertyInfo.PropertyType) == null);
    }

    private static bool TryGetEnumerableElementType(Type type, out Type elementType)
    {
        if (type.IsArray)
        {
            elementType = type.GetElementType();
            return true;
        }

        if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type.IsGenericType && type.GenericTypeArguments.Length == 1)
        {
            elementType = type.GenericTypeArguments[0];
            return true;
        }

        elementType = null;
        return false;
    }

    #endregion Methods
}
