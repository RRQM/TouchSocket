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

using System.Runtime.CompilerServices;
using TouchSocket.Core;

namespace TouchSocket.Semi;

/// <summary>
/// 表示 HSMS 消息头（10 字节）。
/// </summary>
public readonly record struct HsmsHeader
{
    /// <summary>
    /// 设备 ID（Device ID）。
    /// </summary>
    public ushort DeviceId { get; init; }

    /// <summary>
    /// Stream 字节（S），高位包含 ReplyBit。
    /// </summary>
    public byte S { get; init; }

    /// <summary>
    /// Function 字节（F）。
    /// </summary>
    public byte F { get; init; }

    /// <summary>
    /// 是否需要回复（Reply Bit）。
    /// </summary>
    public bool ReplyExpected { get; init; }

    /// <summary>
    /// PType 字节（Presentation Type），固定为 0x00。
    /// </summary>
    public byte PType { get; init; }

    /// <summary>
    /// SType 字节（Session Type，即消息类型）。
    /// </summary>
    public HsmsMessageType MessageType { get; init; }

    /// <summary>
    /// 消息系统字节（System Bytes / Message ID）。
    /// </summary>
    public int SystemBytes { get; init; }

    internal static HsmsHeader Read<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var deviceId = TouchSocketBitConverter.BigEndian.To<ushort>(reader.GetSpan(2));
        reader.Advance(2);
        var sByte = reader.GetSpan(1)[0];
        reader.Advance(1);
        var fByte = reader.GetSpan(1)[0];
        reader.Advance(1);
        var pType = reader.GetSpan(1)[0];
        reader.Advance(1);
        var sType = (HsmsMessageType)reader.GetSpan(1)[0];
        reader.Advance(1);
        var systemBytes = TouchSocketBitConverter.BigEndian.To<int>(reader.GetSpan(4));
        reader.Advance(4);
        var replyExpected = (sByte & 0x80) != 0;
        return new HsmsHeader
        {
            DeviceId = deviceId,
            S = (byte)(sByte & 0x7F),
            F = fByte,
            ReplyExpected = replyExpected,
            PType = pType,
            MessageType = sType,
            SystemBytes = systemBytes
        };
    }

    internal static void Write<TWriter>(ref TWriter writer, in HsmsHeader header)
        where TWriter : IBytesWriter
    {
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, header.DeviceId, EndianType.Big);
        var sByte = (byte)(header.S | (header.ReplyExpected ? 0x80 : 0x00));
        WriterExtension.WriteValue<TWriter, byte>(ref writer, sByte);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, header.F);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, header.PType);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)header.MessageType);
        WriterExtension.WriteValue<TWriter, int>(ref writer, header.SystemBytes, EndianType.Big);
    }
}
