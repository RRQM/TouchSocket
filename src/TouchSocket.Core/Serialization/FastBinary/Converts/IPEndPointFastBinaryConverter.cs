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

using System.Net;

namespace TouchSocket.Core;

internal class IPEndPointFastBinaryConverter : FastBinaryConverter<IPEndPoint>
{
    protected override IPEndPoint Read<TReader>(ref TReader reader, Type type)
    {
        var bytes = ReaderExtension.ReadByteSpan(ref reader).ToArray();
        var scopeId = ReaderExtension.ReadValue<TReader, long>(ref reader);
        var port = ReaderExtension.ReadValue<TReader, int>(ref reader);
        var address = bytes.Length == 16 ? new IPAddress(bytes, scopeId) : new IPAddress(bytes);

        return new IPEndPoint(address, port);
    }

    protected override void Write<TWriter>(ref TWriter writer, in IPEndPoint obj)
    {
        var bytes = obj.Address.GetAddressBytes();

        WriterExtension.WriteByteSpan(ref writer, bytes);
        WriterExtension.WriteValue<TWriter, long>(ref writer, bytes.Length == 16 ? obj.Address.ScopeId : 0);
        WriterExtension.WriteValue<TWriter, int>(ref writer, obj.Port);
    }
}