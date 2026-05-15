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

namespace TouchSocket.SocketIo;

internal class EngineIo4 : IEngineIo
{
    public EngineIo4(EngineIoTransportType engineIOTransportType)
    {
        this.EngineIoTransportType = engineIOTransportType;
    }

    public EngineIoTransportType EngineIoTransportType { get; private set; }

    #region Encode

    public void EncodeToBinary<TWriter>(EngineIoMessage message, ref TWriter writer)
        where TWriter : IBytesWriter
    {
        var typeSpan = writer.GetSpan(1);
        typeSpan[0] = (byte)message.MessageType;
        writer.Advance(1);
        writer.Write(message.RawData.Span);
    }

    public void EncodeToText<TWriter>(EngineIoMessage message, ref TWriter writer)
        where TWriter : IBytesWriter
    {
        if (message.IsText)
        {
            var span = writer.GetSpan(1);
            span[0] = (byte)('0' + (int)message.MessageType);
            writer.Advance(1);
            writer.Write(message.RawData.Span);
        }
        else
        {
            WriterExtension.WriteNormalString(ref writer, "b", Encoding.UTF8);
            WriterExtension.WriteBase64(ref writer, message.RawData.Span);
        }
    }

    #endregion Encode

    #region Decode

    public const string Seperator = "\u001e";

    public EngineIoMessage Decode(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(data));
        }

        var firstByte = data.Span[0];
        var type = (EngineIoMessageType)(firstByte - '0');
        var rest = data.Slice(1);
        return rest.IsEmpty
            ? new EngineIoMessage(type, true, ReadOnlyMemory<byte>.Empty)
            : new EngineIoMessage(type, true, rest);
    }
    #endregion Decode
}
