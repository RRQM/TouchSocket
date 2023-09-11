using Newtonsoft.Json;
using System.Collections.Generic;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiRoot
    {
        [JsonProperty("openapi")]
        public string OpenApi { get; set; } = "3.0.1";

        [JsonProperty("info")]
        public OpenApiInfo Info { get; set; }

        [JsonProperty("paths")]
        public Dictionary<string, OpenApiPath> Paths { get; set; }

        [JsonProperty("components")]
        public OpenApiComponent Components { get; set; }
    }
}
