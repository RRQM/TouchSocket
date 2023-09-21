using Newtonsoft.Json;
using System.Collections.Generic;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiSchema
    {
        [JsonProperty("$ref")]
        public string Ref { get; set; }

        [JsonProperty("type")]
        public OpenApiDataTypes? Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, OpenApiProperty> Properties { get; set; }

        [JsonProperty("items")]
        public OpenApiSchema Items { get; set; }

        [JsonProperty("enum")]
        public int[] Enum { get; set; }
    }
}