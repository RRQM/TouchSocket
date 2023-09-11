using Newtonsoft.Json;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; } = "API V1";

        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";
    }
}
