using Newtonsoft.Json;
using System.Collections.Generic;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiRequestBody
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("content")]
        public Dictionary<string, OpenApiContent> Content { get; set; }
    }
}
