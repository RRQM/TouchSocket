using Newtonsoft.Json;
using System.Collections.Generic;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiComponent
    {
        [JsonProperty("schemas")]
        public Dictionary<string, OpenApiSchema> Schemas { get; set; }
    }
}