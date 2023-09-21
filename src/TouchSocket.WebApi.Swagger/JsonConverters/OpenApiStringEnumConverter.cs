using Newtonsoft.Json.Converters;

namespace TouchSocket.WebApi.Swagger
{
    internal class OpenApiStringEnumConverter : StringEnumConverter
    {
        public OpenApiStringEnumConverter() : base(true)
        {
        }
    }
}