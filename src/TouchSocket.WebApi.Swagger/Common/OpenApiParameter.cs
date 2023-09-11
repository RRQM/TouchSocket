using Newtonsoft.Json;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiParameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("in")]
        public string In { get; set; }

        [JsonProperty("schema")]
        public OpenApiSchema Schema { get; set; }
    }
}
