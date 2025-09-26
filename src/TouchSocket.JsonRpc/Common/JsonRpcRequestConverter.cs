//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TouchSocket.JsonRpc;

internal class JsonRpcRequestConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(InternalJsonRpcRequest);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var jsonRpcRequest = new InternalJsonRpcRequest();

        // Load the JObject from the reader
        var jsonObject = JObject.Load(reader);
        if (!jsonObject.TryGetValue("jsonrpc", StringComparison.OrdinalIgnoreCase, out var tokenJsonrpc))
        {
            return default;
        }
        // Deserialize properties
        jsonRpcRequest.Jsonrpc = tokenJsonrpc.Value<string>();

        if (!jsonObject.TryGetValue("method", StringComparison.OrdinalIgnoreCase, out var tokenMethod))
        {
            return default;
        }
        jsonRpcRequest.Method = tokenMethod.Value<string>();
        if (!jsonObject.TryGetValue("params", StringComparison.OrdinalIgnoreCase, out var tokenParams))
        {
            return default;
        }

        jsonRpcRequest.ParamsObject = tokenParams;

        if (!jsonObject.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out var tokenId))
        {
            return default;
        }

        jsonRpcRequest.Id = tokenId.Value<int?>();

        return jsonRpcRequest;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var jsonRpcRequest = (InternalJsonRpcRequest)value;
        writer.WriteStartObject();
        writer.WritePropertyName("jsonrpc");
        writer.WriteValue(jsonRpcRequest.Jsonrpc);
        writer.WritePropertyName("method");
        writer.WriteValue(jsonRpcRequest.Method);
        writer.WritePropertyName("params");
        writer.WriteStartArray();
        foreach (var item in jsonRpcRequest.ParamsStrings)
        {
            writer.WriteRawValue(item);
        }
        writer.WriteEndArray();
        writer.WritePropertyName("id");
        writer.WriteValue(jsonRpcRequest.Id);
        writer.WriteEndObject();
    }
}