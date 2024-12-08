#if SystemTextJson
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.WebApi;

internal sealed class WebApiSystemTextJsonSerializerFormatter : ISerializerFormatter<string, HttpContext>
{
    private readonly JsonSerializerOptions m_jsonSerializerOptions;

    public WebApiSystemTextJsonSerializerFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        this.m_jsonSerializerOptions = jsonSerializerOptions;
    }

    public int Order { get; set; }

    public bool TryDeserialize(HttpContext state, in string source, Type targetType, out object target)
    {
        try
        {
            target = System.Text.Json.JsonSerializer.Deserialize(source, targetType, this.m_jsonSerializerOptions);
            return true;
        }
        catch
        {
            target = default;
            return false;
        }
    }

    public bool TrySerialize(HttpContext state, in object target, out string source)
    {
        switch (state.Request.Accept)
        {
            case "application/xml":
            case "text/xml":
                {
                    source = default;
                    return false;
                }
            case "application/json":
            case "text/json":
            case "text/plain":
            default:
                {
                    try
                    {
                        source = System.Text.Json.JsonSerializer.Serialize(target, target.GetType(), this.m_jsonSerializerOptions);
                        return true;
                    }
                    catch
                    {
                        source = default;
                        return false;
                    }
                }
        }
    }
}

#endif