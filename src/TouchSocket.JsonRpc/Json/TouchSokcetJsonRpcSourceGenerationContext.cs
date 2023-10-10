#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// TouchSokcetJsonRpcSourceGenerationContext
    /// </summary>
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(JsonRpcResponseContext))]
    [JsonSerializable(typeof(JsonRpcError))]
    [JsonSerializable(typeof(JsonRpcRequestContext))]
    [JsonSerializable(typeof(JsonRpcSuccessResponse))]
    [JsonSerializable(typeof(JsonRpcErrorResponse))]
    [JsonSerializable(typeof(JsonRpcWaitResult))]
    [JsonSerializable(typeof(object))]
    internal partial class TouchSokcetJsonRpcSourceGenerationContext : JsonSerializerContext
    {
    }
}
#endif

