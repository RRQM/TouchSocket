#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;

namespace TouchSocket.Core
{
    /// <summary>
    /// TouchSokcetCoreSourceGenerationContext
    /// </summary>
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Metadata))]
    internal partial class TouchSokcetCoreSourceGenerationContext : JsonSerializerContext
    {

    }
}

#endif
