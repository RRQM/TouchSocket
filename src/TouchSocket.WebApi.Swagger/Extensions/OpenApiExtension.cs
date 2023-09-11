namespace TouchSocket.WebApi.Swagger
{
    internal static class OpenApiExtension
    {
        public static bool IsPrimitive(this OpenApiDataTypes dataTypes)
        {
            switch (dataTypes)
            {
                case OpenApiDataTypes.String:
                case OpenApiDataTypes.Number:
                case OpenApiDataTypes.Integer:
                case OpenApiDataTypes.Boolean:
                    return true;
                case OpenApiDataTypes.Binary:
                case OpenApiDataTypes.BinaryCollection:
                case OpenApiDataTypes.Record:
                case OpenApiDataTypes.Tuple:
                case OpenApiDataTypes.Array:
                case OpenApiDataTypes.Object:
                case OpenApiDataTypes.Struct:
                case OpenApiDataTypes.Any:
                default:
                    return false;
            }
        }
    }
}
