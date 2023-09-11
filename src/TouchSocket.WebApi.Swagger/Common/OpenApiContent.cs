using Newtonsoft.Json;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiContent
    {
        [JsonProperty("schema")]
        public OpenApiSchema Schema { get; set; }
    }
}
