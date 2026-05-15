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
using System.Text.Json.Nodes;

namespace TouchSocket.SocketIo;

/// <summary>
/// 基于 <see cref="System.Text.Json"/> 的 Socket.IO 序列化器。
/// </summary>
public class SystemTextJsonSerializer : ISocketIoSerializer
{
    private static readonly JsonSerializerOptions s_options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 构建包含文本和二进制数据项的列表。
    /// </summary>
    public static List<DataItem> NewDataItems(StringBuilder builder, IEnumerable<ReadOnlyMemory<byte>> bytes)
    {
        var result = new List<DataItem>
        {
            new DataItem(builder.ToString())
        };
        result.AddRange(bytes.Select(x => new DataItem(x)));
        return result;
    }

    private static ISocketIoMessage CreateMessage(SocketIoMessageType messageType)
    {
        return new SystemTextJsonSocketIoMessage(messageType);
    }

    /// <inheritdoc/>
    public ISocketIoMessage Decode(in EngineIoMessage message)
    {
        if (!message.IsText)
        {
            return null;
        }

        var text = message.RawData.Span.ToUtf8String();
        if (text.IsNullOrEmpty())
        {
            return null;
        }

        var socketIOMessage = (SystemTextJsonSocketIoMessage)CreateMessage((SocketIoMessageType)(text[0] - '0'));
        var body = text.Substring(1);
        if (body.HasValue())
        {
            ReadMessage(socketIOMessage, body);
        }
        return socketIOMessage;
    }

    /// <inheritdoc/>
    public object Deserialize(Type targetType, in ISocketIoMessage message, int index)
    {
        var msg = (SystemTextJsonSocketIoMessage)message;
        var jsonNode = msg.JsonArray[index];
        return jsonNode.Deserialize(targetType, s_options);
    }

    /// <inheritdoc/>
    public IConnectMessage DeserializeHandshakeMessage(in EngineIoMessage message)
    {
        var handshakeMessage = JsonSerializer.Deserialize<SystemTextJsonHandshakeMessage>(message.RawData.Span.ToUtf8String(), s_options)
            ?? throw new InvalidOperationException("握手消息反序列化失败");
        handshakeMessage.Validate();
        return handshakeMessage;
    }

    /// <inheritdoc/>
    public List<DataItem> SerializeAck(int? packetId, string nsp, object[] data)
    {
        var bytes = new List<ReadOnlyMemory<byte>>();
        var json = data.Length > 0 ? SerializeWithBinaryPlaceholders(data, bytes) : "[]";

        var builder = new StringBuilder();
        if (bytes.Count == 0)
        {
            builder.Append("43");
        }
        else
        {
            builder.Append("46").Append(bytes.Count).Append('-');
        }

        if (!string.IsNullOrEmpty(nsp))
        {
            builder.Append(nsp).Append(',');
        }

        builder.Append(packetId).Append(json);
        return NewDataItems(builder, bytes);
    }

    /// <inheritdoc/>
    public List<DataItem> SerializeEvent(string eventName, int? packetId, string nsp, object[] data)
    {
        var newData = new object[data.Length + 1];
        newData[0] = eventName;
        Array.Copy(data, 0, newData, 1, data.Length);

        var bytes = new List<ReadOnlyMemory<byte>>();
        var json = SerializeWithBinaryPlaceholders(newData, bytes);

        var builder = new StringBuilder();
        if (bytes.Count == 0)
        {
            builder.Append("42");
        }
        else
        {
            builder.Append("45").Append(bytes.Count).Append('-');
        }

        if (!string.IsNullOrEmpty(nsp))
        {
            builder.Append(nsp).Append(',');
        }

        if (packetId.HasValue)
        {
            builder.Append(packetId);
        }

        builder.Append(json);
        return NewDataItems(builder, bytes);
    }

    private static string SerializeWithBinaryPlaceholders(object[] data, List<ReadOnlyMemory<byte>> bytes)
    {
        var nodes = new JsonArray();
        foreach (var item in data)
        {
            if (item is byte[] byteArray)
            {
                bytes.Add(byteArray);
                var placeholder = new JsonObject
                {
                    ["_placeholder"] = JsonValue.Create(true),
                    ["num"] = JsonValue.Create(bytes.Count - 1)
                };
                nodes.Add(placeholder);
            }
            else if (item is ReadOnlyMemory<byte> romBytes)
            {
                bytes.Add(romBytes);
                var placeholder = new JsonObject
                {
                    ["_placeholder"] = JsonValue.Create(true),
                    ["num"] = JsonValue.Create(bytes.Count - 1)
                };
                nodes.Add(placeholder);
            }
            else
            {
                nodes.Add(JsonSerializer.SerializeToNode(item, s_options));
            }
        }
        return nodes.ToJsonString();
    }

    #region 读取消息

    private static void ReadAckMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        var index = text.IndexOf('[');
        if (index < 0)
        {
            return;
        }

        var lastIndex = text.LastIndexOf(',', index);
        if (lastIndex > -1)
        {
            var subText = text.Substring(0, index);
            message.Namespace = subText.Substring(0, lastIndex);
            var idStr = subText.Substring(lastIndex + 1);
            if (idStr.HasValue() && int.TryParse(idStr, out var id))
            {
                message.Id = id;
            }
        }
        else
        {
            var idStr = text.Substring(0, index);
            if (idStr.HasValue() && int.TryParse(idStr, out var id))
            {
                message.Id = id;
            }
        }

        message.Text = text.Substring(index);
    }

    private static void ReadBinaryAckMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        var index1 = text.IndexOf('-');
        if (index1 < 0)
        {
            return;
        }

        message.BytesCount = int.Parse(text.Substring(0, index1));

        var index2 = text.IndexOf('[', index1);
        if (index2 < 0)
        {
            return;
        }

        var index3 = text.LastIndexOf(',', index2);
        if (index3 > index1)
        {
            message.Namespace = text.Substring(index1 + 1, index3 - index1 - 1);
            var idStr = text.Substring(index3 + 1, index2 - index3 - 1);
            if (idStr.HasValue() && int.TryParse(idStr, out var id))
            {
                message.Id = id;
            }
        }
        else
        {
            var idStr = text.Substring(index1 + 1, index2 - index1 - 1);
            if (idStr.HasValue() && int.TryParse(idStr, out var id))
            {
                message.Id = id;
            }
        }

        message.Text = text.Substring(index2);
    }

    private static void ReadBinaryMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        message.Bytes = new List<ReadOnlyMemory<byte>>();

        var index1 = text.IndexOf('-');
        if (index1 < 0)
        {
            return;
        }

        message.BytesCount = int.Parse(text.Substring(0, index1));

        var index2 = text.IndexOf('[', index1);
        if (index2 < 0)
        {
            return;
        }

        var index3 = text.LastIndexOf(',', index2);
        if (index3 > index1)
        {
            message.Namespace = text.Substring(index1 + 1, index3 - index1 - 1);
            var idLength = index2 - index3 - 1;
            if (idLength > 0)
            {
                message.Id = int.Parse(text.Substring(index3 + 1, idLength));
            }
        }
        else
        {
            var idLength = index2 - index1 - 1;
            if (idLength > 0)
            {
                message.Id = int.Parse(text.Substring(index1 + 1, idLength));
            }
        }

        message.Text = text.Substring(index2);
    }

    private static void ReadConnectedMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        var index = text.IndexOf('{');
        message.Namespace = index > 0 ? text.Substring(0, index - 1) : string.Empty;
    }

    private static void ReadDisconnectedMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        message.Namespace = text.TrimEnd(',');
    }

    private static void ReadErrorMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        var index = text.IndexOf('{');
        if (index > 0)
        {
            message.Namespace = text.Substring(0, index - 1);
            text = text.Substring(index);
        }

        var jsonObject = JsonNode.Parse(text) as JsonObject;
        message.Error = jsonObject?["message"]?.GetValue<string>();
    }

    private static void ReadEventMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        var index = text.IndexOf('[');
        if (index < 0)
        {
            return;
        }

        var lastIndex = text.LastIndexOf(',', index);
        if (lastIndex > -1)
        {
            var subText = text.Substring(0, index);
            message.Namespace = subText.Substring(0, lastIndex);
            var idStr = subText.Substring(lastIndex + 1);
            if (idStr.HasValue() && int.TryParse(idStr, out var id))
            {
                message.Id = id;
            }
        }
        else
        {
            if (index > 0)
            {
                var idStr = text.Substring(0, index);
                if (int.TryParse(idStr, out var id))
                {
                    message.Id = id;
                }
            }
        }

        message.Text = text.Substring(index);
    }

    private static void ReadMessage(SystemTextJsonSocketIoMessage message, string text)
    {
        switch (message.MessageType)
        {
            case SocketIoMessageType.Connected:
                ReadConnectedMessage(message, text);
                break;

            case SocketIoMessageType.Disconnected:
                ReadDisconnectedMessage(message, text);
                break;

            case SocketIoMessageType.Event:
                ReadEventMessage(message, text);
                break;

            case SocketIoMessageType.Ack:
                ReadAckMessage(message, text);
                break;

            case SocketIoMessageType.Error:
                ReadErrorMessage(message, text);
                break;

            case SocketIoMessageType.Binary:
                ReadBinaryMessage(message, text);
                break;

            case SocketIoMessageType.BinaryAck:
                ReadBinaryAckMessage(message, text);
                break;

            default:
                break;
        }
    }

    #endregion 读取消息
}
