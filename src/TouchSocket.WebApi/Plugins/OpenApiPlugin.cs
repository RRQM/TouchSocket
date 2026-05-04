// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.WebApi.OpenApi;

namespace TouchSocket.WebApi;

/// <summary>
/// 提供 OpenApi JSON 端点的插件。
/// 在首次收到 HTTP 请求时懒加载并缓存 openapi.json，路径为 <c>/{Prefix}/openapi.json</c>。
/// </summary>
public class OpenApiPlugin : PluginBase, IHttpPlugin
{
    private readonly OpenApiOption m_options;
    private volatile bool m_initialized;
    private readonly Lock m_initLock = new();
    private string m_openApiJsonPath;
    private ReadOnlyMemory<byte> m_openApiJsonBytes;

    /// <summary>
    /// 初始化 <see cref="OpenApiPlugin"/> 的新实例。
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="options">OpenApi 配置选项</param>
    public OpenApiPlugin(ILog logger, OpenApiOption options)
    {
        this.Logger = logger;
        this.m_options = options;
        this.Prefix = options.Prefix.IsNullOrEmpty() ? "openapi" : options.Prefix;
    }

    /// <summary>
    /// 获取日志记录器。
    /// </summary>
    protected ILog Logger { get; }

    /// <summary>
    /// 获取访问 OpenApi 的路径前缀。
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// 获取规范化的前缀路径（以 "/" 开头）。
    /// </summary>
    public string GetPrefixPath()
    {
        return this.Prefix.IsNullOrEmpty() ? string.Empty
            : (this.Prefix.StartsWith("/") ? this.Prefix : $"/{this.Prefix}");
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        this.EnsureInitialized();
        if (e.Context.Request.UrlEquals(this.m_openApiJsonPath))
        {
            e.Handled = true;
            e.Context.Response
                .SetStatusWithSuccess()
                .SetContentTypeByExtension(Path.GetExtension(m_openApiJsonPath))
                .SetContent(this.m_openApiJsonBytes);
            await e.Context.Response.AnswerAsync().ConfigureDefaultAwait();
            return;
        }

        await e.InvokeNext().ConfigureDefaultAwait();
    }

    private void EnsureInitialized()
    {
        if (this.m_initialized)
        {
            return;
        }

        lock (this.m_initLock)
        {
            if (this.m_initialized)
            {
                return;
            }

            try
            {
                var webApiPlugin = this.PluginManager?.Plugins.OfType<WebApiParserPlugin>().FirstOrDefault();
                if (webApiPlugin != null)
                {
                    var prefix = this.GetPrefixPath();
                    this.m_openApiJsonPath = $"{prefix}/openapi.json";
                    this.m_openApiJsonBytes = this.BuildOpenApiJson(webApiPlugin);
                }
                else
                {
                    this.Logger?.Warning($"该服务器中似乎没有添加{nameof(WebApiParserPlugin)}。");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Exception(this, ex);
            }

            this.m_initialized = true;
        }
    }

    #region Build

    private IEnumerable<string> GetTags(RpcMethod rpcMethod)
    {
        if (this.m_options.GetTags != null)
        {
            return this.m_options.GetTags(rpcMethod);
        }

        return new[] { rpcMethod.ServerFromType.Name };
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "OpenApi内部使用，相信动态代码是有效的")]
    private void AddSchemaType(Type type, in List<Type> types)
    {
        if (type.IsArray)
        {
            type = type.GetElementType();
        }
        else if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType && type.GenericTypeArguments.Length == 1)
        {
            type = type.GetGenericArguments()[0];
        }

        if (IsSimpleType(type))
        {
            return;
        }

        if (types.Contains(type))
        {
            return;
        }

        if (this.ParseDataTypes(type) != OpenApiDataTypes.Object)
        {
            return;
        }

        types.Add(type);

        foreach (var item in type.GetProperties())
        {
            this.AddSchemaType(item.PropertyType, types);
        }
    }

    private static bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive || type == TouchSocketCoreUtility.StringType)
        {
            return true;
        }

        if (type.IsNullableType(out var actualType))
        {
            return IsSimpleType(actualType);
        }

        return false;
    }

    private ReadOnlyMemory<byte> BuildOpenApiJson(WebApiParserPlugin webApiParserPlugin)
    {
        var openApiRoot = new OpenApiRoot();
        openApiRoot.Info = new OpenApiInfo();

        var paths = new Dictionary<string, OpenApiPath>();
        var schemaTypeList = new List<Type>();
        var rpcMethods = new List<RpcMethod>();

        foreach (var mappingMethod in webApiParserPlugin.Mapping)
        {
            if (rpcMethods.Contains(mappingMethod.RpcMethod))
            {
                continue;
            }

            if (mappingMethod.IsRegex)
            {
                continue;
            }

            this.BuildHttpMethod(mappingMethod.Url, mappingMethod.HttpMethod, mappingMethod.RpcMethod, schemaTypeList, paths);
            rpcMethods.Add(mappingMethod.RpcMethod);
        }

        openApiRoot.Paths = paths;
        openApiRoot.Components = this.GetComponents(schemaTypeList);

        return JsonSerializer.Serialize(openApiRoot, OpenApiJsonSerializerContext.Default.OpenApiRoot).ToUtf8Bytes();
    }

    private void BuildHttpMethod(string url, HttpMethod httpMethod, RpcMethod rpcMethod, in List<Type> schemaTypeList, in Dictionary<string, OpenApiPath> paths)
    {
        var openApiPathValue = new OpenApiPathValue
        {
            Tags = this.GetTags(rpcMethod),
            Description = rpcMethod.GetDescription(),
            Summary = rpcMethod.GetDescription()
        };

        var openApiParameters = new List<OpenApiParameter>();

        var parameters = this.GetWebApiParameter(rpcMethod);
        if (parameters.Count > 0)
        {
            var simpleParameters = parameters.Where(a =>
            {
                if (!this.ParseDataTypes(a.Parameter.Type).IsPrimitive())
                {
                    return false;
                }

                if (a.IsFromBody || a.IsFromForm)
                {
                    return false;
                }

                return true;
            });

            foreach (var parameter in simpleParameters)
            {
                var openApiParameter = this.GetParameter(parameter.Parameter.ParameterInfo);
                this.AddSchemaType(parameter.Parameter.Type, schemaTypeList);
                openApiParameters.Add(openApiParameter);
            }

            var forms = parameters.Where(a => a.IsFromForm);
            if (forms.Any())
            {
                var body = new OpenApiRequestBody();
                body.Content = new Dictionary<string, OpenApiContent>();
                var content = new OpenApiContent();
                var properties = new Dictionary<string, OpenApiProperty>();

                foreach (var item in forms)
                {
                    var openApiProperty = this.CreateProperty(item.Parameter.Type, item.Parameter.ParameterInfo.GetDescription());
                    properties.Add(this.GetOpenApiParameterName(item.Parameter.ParameterInfo), openApiProperty);
                }

                content.Schema = new OpenApiSchema()
                {
                    Type = OpenApiDataTypes.Object,
                    Properties = properties
                };
                body.Content.Add("multipart/form-data", content);
                openApiPathValue.RequestBody = body;
            }
            else
            {
                var last = parameters.Where(a => a.IsFromBody).FirstOrDefault();
                if (last is null)
                {
                    if (!parameters.Last().Parameter.Type.IsPrimitive())
                    {
                        last = parameters.Last();
                    }
                }

                if (last is not null)
                {
                    this.AddSchemaType(last.Parameter.Type, schemaTypeList);

                    var body = new OpenApiRequestBody();
                    body.Content = new Dictionary<string, OpenApiContent>();
                    var content = new OpenApiContent();
                    content.Schema = this.CreateSchema(last.Parameter.Type);
                    body.Content.Add("application/json", content);
                    body.Content.Add("text/xml", content);
                    body.Content.Add("text/plain", content);
                    body.Content.Add("text/json", content);
                    body.Content.Add("application/xml", content);
                    openApiPathValue.RequestBody = body;
                }
            }
        }

        openApiPathValue.Parameters = openApiParameters.Count > 0 ? openApiParameters : default;

        this.BuildResponse(rpcMethod, openApiPathValue, schemaTypeList);

        this.m_options.ConfigureOperation?.Invoke(rpcMethod, openApiPathValue);

        if (!paths.TryGetValue(url, out var openApiPath))
        {
            openApiPath = new OpenApiPath();
            paths.Add(url, openApiPath);
        }

        openApiPath.Add(httpMethod.ToString().ToLower(), openApiPathValue);
    }

    private List<WebApiParameterInfo> GetWebApiParameter(RpcMethod rpcMethod)
    {
        return rpcMethod.GetNormalParameters()
            .Select(a => new WebApiParameterInfo(a)).ToList();
    }

    private void BuildResponse(RpcMethod rpcMethod, in OpenApiPathValue openApiPathValue, in List<Type> schemaTypeList)
    {
        openApiPathValue.Responses = new Dictionary<string, OpenApiResponse>();

        var producesAttributes = rpcMethod.Info.GetCustomAttributes<WebApiProducesResponseTypeAttribute>(false);
        if (producesAttributes.Any())
        {
            foreach (var attr in producesAttributes)
            {
                var producesResponse = new OpenApiResponse();
                producesResponse.Description = attr.StatusCode == 200 ? "Success" : attr.StatusCode.ToString();
                producesResponse.Content = new Dictionary<string, OpenApiContent>();
                var producesContent = new OpenApiContent();
                producesContent.Schema = this.CreateSchema(attr.Type);
                producesResponse.Content.Add("application/json", producesContent);
                producesResponse.Content.Add("text/xml", producesContent);
                producesResponse.Content.Add("text/plain", producesContent);
                producesResponse.Content.Add("text/json", producesContent);
                producesResponse.Content.Add("application/xml", producesContent);
                this.AddSchemaType(attr.Type, schemaTypeList);
                openApiPathValue.Responses.TryAdd(attr.StatusCode.ToString(), producesResponse);
            }

            return;
        }

        var openApiResponse = new OpenApiResponse();
        openApiResponse.Description = "Success";

        if (rpcMethod.HasReturn)
        {
            openApiResponse.Content = new Dictionary<string, OpenApiContent>();
            var openApiContent = new OpenApiContent();
            openApiResponse.Content.Add("application/json", openApiContent);
            openApiResponse.Content.Add("text/xml", openApiContent);
            openApiResponse.Content.Add("text/plain", openApiContent);
            openApiResponse.Content.Add("text/json", openApiContent);
            openApiResponse.Content.Add("application/xml", openApiContent);
            openApiContent.Schema = this.CreateSchema(rpcMethod.RealReturnType);
            this.AddSchemaType(rpcMethod.RealReturnType, schemaTypeList);
        }

        openApiPathValue.Responses.Add("200", openApiResponse);
    }

    private OpenApiProperty CreateProperty(Type type, string description)
    {
        var openApiProperty = new OpenApiProperty();
        var dataTypes = this.ParseDataTypes(type);
        openApiProperty.Description = description;

        switch (dataTypes)
        {
            case OpenApiDataTypes.String:
            case OpenApiDataTypes.Number:
            case OpenApiDataTypes.Boolean:
            case OpenApiDataTypes.Integer:
                openApiProperty.Type = dataTypes;
                break;

            case OpenApiDataTypes.Array:
                openApiProperty.Type = dataTypes;
                var elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                openApiProperty.Items = this.CreateProperty(elementType, elementType.GetDescription());
                break;

            case OpenApiDataTypes.Object:
                openApiProperty.Ref = $"#/components/schemas/{type.Name}";
                break;
        }

        openApiProperty.Format = this.GetSchemaFormat(type);
        return openApiProperty;
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "OpenApi内部使用，相信动态代码是有效的")]
    private OpenApiSchema CreateSchema(Type type)
    {
        var schema = new OpenApiSchema();
        var dataTypes = this.ParseDataTypes(type);

        switch (dataTypes)
        {
            case OpenApiDataTypes.String:
            case OpenApiDataTypes.Number:
            case OpenApiDataTypes.Boolean:
            case OpenApiDataTypes.Integer:
                schema.Type = dataTypes;
                break;

            case OpenApiDataTypes.Array:
                schema.Type = dataTypes;
                schema.Items = this.CreateSchema(type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0]);
                break;

            case OpenApiDataTypes.Object:
                if (type.IsEnum)
                {
                    var list = new List<long>();
                    foreach (var item in Enum.GetValues(type))
                    {
                        list.Add(Convert.ToInt64(item));
                    }

                    schema.Enum = list.ToArray();
                    schema.Type = OpenApiDataTypes.Integer;
                }
                else
                {
                    schema.Ref = $"#/components/schemas/{this.GetSchemaName(type)}";
                }

                break;
        }

        schema.Format = this.GetSchemaFormat(type);
        return schema;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "OpenApi内部使用，相信动态代码是有效的")]
    private OpenApiComponent GetComponents(List<Type> types)
    {
        if (types.Count == 0)
        {
            return default;
        }

        var components = new OpenApiComponent();
        components.Schemas = new Dictionary<string, OpenApiSchema>();

        foreach (var type in types)
        {
            var schema = this.CreateSchema(type);
            var properties = new Dictionary<string, OpenApiProperty>();

            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (properties.ContainsKey(propertyInfo.Name))
                {
                    if (propertyInfo.DeclaringType == type)
                    {
                        properties[propertyInfo.Name] = this.CreateProperty(propertyInfo.PropertyType, propertyInfo.GetDescription());
                    }
                }
                else
                {
                    properties.Add(propertyInfo.Name, this.CreateProperty(propertyInfo.PropertyType, propertyInfo.GetDescription()));
                }
            }

            schema.Properties = properties.Count == 0 ? default : properties;
            components.Schemas.TryAdd(this.GetSchemaName(type), schema);
        }

        return components;
    }

    private string GetIn(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        if (!IsSimpleType(type))
        {
            return default;
        }

        if (parameter.IsDefined(typeof(FromHeaderAttribute)))
        {
            return "header";
        }
        else if (parameter.IsDefined(typeof(FromBodyAttribute)))
        {
            return "body";
        }
        else if (parameter.IsDefined(typeof(FromFormAttribute)))
        {
            return "formData";
        }
        else
        {
            return "query";
        }
    }

    private OpenApiParameter GetParameter(ParameterInfo parameterInfo)
    {
        var openApiParameter = new OpenApiParameter();
        openApiParameter.Name = this.GetOpenApiParameterName(parameterInfo);
        openApiParameter.Description = parameterInfo.GetDescription();
        openApiParameter.In = this.GetIn(parameterInfo);
        openApiParameter.Schema = this.CreateSchema(parameterInfo.ParameterType);
        return openApiParameter;
    }

    private string GetOpenApiParameterName(ParameterInfo parameter)
    {
        foreach (var item in parameter.GetCustomAttributes())
        {
            if (item is WebApiNameAttribute attribute && attribute.Name.HasValue())
            {
                return attribute.Name;
            }
        }

        return parameter.Name;
    }

    private string GetSchemaFormat(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 => "int32",
            TypeCode.Int64 or TypeCode.UInt64 => "int64",
            TypeCode.Single => "float",
            TypeCode.Double or TypeCode.Decimal => "double",
            TypeCode.DateTime => "date-time",
            TypeCode.Boolean or TypeCode.Char or TypeCode.String => default,
            _ => "object",
        };
    }

    private string GetSchemaName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var stringBuilder = new StringBuilder();
        foreach (var item in type.GetGenericArguments())
        {
            stringBuilder.Append(this.GetSchemaName(item));
        }

        stringBuilder.Append(type.Name.Substring(0, type.Name.IndexOf('`')));
        return stringBuilder.ToString();
    }

    private static bool IsFormFileCollection(Type type)
    {
        if (typeof(IMultifileCollection).IsAssignableFrom(type))
        {
            return true;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType != null && typeof(IFormFile).IsAssignableFrom(elementType))
                {
                    return true;
                }
            }
            else
            {
                if (type.IsGenericType && type.GenericTypeArguments.Length == 1 && typeof(IFormFile).IsAssignableFrom(type.GenericTypeArguments[0]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private OpenApiDataTypes ParseDataTypes(Type type)
    {
        if (type is null)
        {
            return OpenApiDataTypes.Any;
        }

        if (type.IsNullableType(out var actualType))
        {
            return this.ParseDataTypes(actualType);
        }

        return type switch
        {
            _ when typeof(string).IsAssignableFrom(type) || typeof(char).IsAssignableFrom(type) => OpenApiDataTypes.String,
            _ when type.IsInteger() => OpenApiDataTypes.Integer,
            _ when type.IsDecimal() => OpenApiDataTypes.Number,
            _ when typeof(bool).IsAssignableFrom(type) => OpenApiDataTypes.Boolean,
            _ when typeof(DateTime).IsAssignableFrom(type) || typeof(DateTimeOffset).IsAssignableFrom(type) => OpenApiDataTypes.String,
            _ when typeof(IFormFile).IsAssignableFrom(type) => OpenApiDataTypes.Binary,
            _ when IsFormFileCollection(type) => OpenApiDataTypes.BinaryCollection,
            _ when type.IsDictionary() => OpenApiDataTypes.Record,
            _ when type.IsValueTuple() => OpenApiDataTypes.Tuple,
            _ when type.IsArray || (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType && type.GenericTypeArguments.Length == 1) => OpenApiDataTypes.Array,
            _ when type.IsEnum || (type != typeof(object) && type.IsClass && Type.GetTypeCode(type) == TypeCode.Object) => OpenApiDataTypes.Object,
            _ when type.IsValueType && !type.IsPrimitive && !type.IsEnum => OpenApiDataTypes.Struct,
            _ => OpenApiDataTypes.Any
        };
    }

    #endregion Build
}

internal static class OpenApiDataTypesExtensions
{
    internal static bool IsPrimitive(this OpenApiDataTypes dataTypes)
    {
        return dataTypes switch
        {
            OpenApiDataTypes.String or OpenApiDataTypes.Number or OpenApiDataTypes.Integer or OpenApiDataTypes.Boolean => true,
            _ => false,
        };
    }
}
