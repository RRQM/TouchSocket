#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// TouchSokcetDmtpSourceGenerationContext
    /// </summary>
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(WaitVerify))]
    [JsonSerializable(typeof(Metadata))]
    [JsonSerializable(typeof(WaitSetId))]
    [JsonSerializable(typeof(WaitPing))]
    internal partial class TouchSokcetDmtpSourceGenerationContext : JsonSerializerContext
    {

    }
}

#endif
