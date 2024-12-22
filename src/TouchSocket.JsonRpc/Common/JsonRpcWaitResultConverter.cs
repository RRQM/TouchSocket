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
using System;

namespace TouchSocket.JsonRpc
{
    internal class JsonRpcWaitResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(InternalJsonRpcWaitResult);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonRpcWaitResult = new InternalJsonRpcWaitResult();
            var jsonObject = JObject.Load(reader);
            jsonRpcWaitResult.Jsonrpc = (string)jsonObject["jsonrpc"];
            jsonRpcWaitResult.Id = (int?)jsonObject["id"];
            if (jsonObject["error"] != null)
            {
                jsonRpcWaitResult.ErrorCode = (int)jsonObject["error"]["code"];
                jsonRpcWaitResult.ErrorMessage = (string)jsonObject["error"]["message"];
            }
            if (jsonObject["result"] != null)
            {
                jsonRpcWaitResult.Result = jsonObject["result"].ToString();
            }

            return jsonRpcWaitResult;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonRpcWaitResult = (InternalJsonRpcWaitResult)value;
            writer.WriteStartObject();
            writer.WritePropertyName("jsonrpc");
            writer.WriteValue(jsonRpcWaitResult.Jsonrpc);
            writer.WritePropertyName("id");
            writer.WriteValue(jsonRpcWaitResult.Id);
            if (jsonRpcWaitResult.ErrorCode == 0)
            {
                // 成功
                writer.WritePropertyName("result");
                writer.WriteRawValue(jsonRpcWaitResult.Result);
            }
            else
            {
                // 失败
                writer.WritePropertyName("error");
                writer.WriteStartObject();
                writer.WritePropertyName("code");
                writer.WriteValue(jsonRpcWaitResult.ErrorCode);
                writer.WritePropertyName("message");
                writer.WriteValue(jsonRpcWaitResult.ErrorMessage);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }
    }
}
