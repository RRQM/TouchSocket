using Newtonsoft.Json;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiProperty
    {
        [JsonProperty("type")]
        public OpenApiDataTypes? Type { get; set; }

        [JsonProperty("readOnly")]
        public bool? ReadOnly { get; set; }

        [JsonProperty("$ref")]
        public string Ref { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("items")]
        public OpenApiProperty Items { get; set; }
    }
}
