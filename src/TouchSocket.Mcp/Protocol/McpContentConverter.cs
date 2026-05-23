//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TouchSocket.Mcp;

/// <summary>
/// 提供 <see cref="McpContent"/> 的多态 JSON 转换器。
/// </summary>
internal sealed class McpContentConverter : JsonConverter<McpContent>
{
    /// <inheritdoc/>
    public override McpContent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp))
        {
            return null;
        }

        var type = typeProp.GetString();
        var raw = root.GetRawText();

        return type switch
        {
            "text" => JsonSerializer.Deserialize<McpTextContent>(raw, options),
            "image" => JsonSerializer.Deserialize<McpImageContent>(raw, options),
            "resource" => JsonSerializer.Deserialize<McpEmbeddedResourceContent>(raw, options),
            _ => JsonSerializer.Deserialize<McpTextContent>(raw, options)
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, McpContent value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
