using Newtonsoft.Json;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcResponseBase
    /// </summary>
    public class JsonRpcResponseBase
    {
        /// <summary>
        /// jsonrpc
        /// </summary>
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonPropertyName("jsonrpc")]
#endif

        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; } = "2.0";

        /// <summary>
        /// id
        /// </summary>
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonPropertyName("id")]
#endif

        [JsonProperty("id")]
        public long? Id { get; set; }
    }
}