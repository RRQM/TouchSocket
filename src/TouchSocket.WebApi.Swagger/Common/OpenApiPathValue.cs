using Newtonsoft.Json;
using System.Collections.Generic;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiPathValue
    {
        [JsonProperty("tags")]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("requestBody")]
        public OpenApiRequestBody RequestBody { get; set; }

        [JsonProperty("parameters")]
        public IEnumerable<OpenApiParameter> Parameters { get; set; }

        [JsonProperty("responses")]
        public Dictionary<string, OpenApiResponse> Responses { get; set; }
    }
}
