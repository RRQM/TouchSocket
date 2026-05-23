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

namespace TouchSocket.Semi;

/// <summary>
/// 表示一条 HSMS 消息，包含消息头和可选的 SECS-II 数据体。
/// </summary>
public class HsmsMessage : IRequestInfo, IBytesBuilder, IWaitHandle
{
    /// <summary>
    /// 初始化 <see cref="HsmsMessage"/> 的新实例。
    /// </summary>
    public HsmsMessage() { }

    /// <summary>
    /// 使用指定的数据体初始化 <see cref="HsmsMessage"/> 的新实例。
    /// </summary>
    /// <param name="body">SECS-II 数据项。</param>
    public HsmsMessage(SecsItem body)
    {
        this.Body = body;
    }

    #region 属性

    /// <summary>
    /// 获取或设置设备 ID。
    /// </summary>
    public ushort DeviceId { get; set; }

    /// <summary>
    /// 获取或设置 Stream 字节（S）。
    /// </summary>
    public byte S { get; set; }

    /// <summary>
    /// 获取或设置 Function 字节（F）。
    /// </summary>
    public byte F { get; set; }

    /// <summary>
    /// 获取或设置是否需要回复。
    /// </summary>
    public bool ReplyExpected { get; set; }

    /// <summary>
    /// 获取或设置 PType 字节（Presentation Type），固定为 0x00。
    /// </summary>
    public byte PType { get; set; }

    /// <summary>
    /// 获取或设置消息类型（SType）。
    /// </summary>
    public HsmsMessageType MessageType { get; set; }

    /// <summary>
    /// 获取或设置消息系统字节（System Bytes / Message ID）。
    /// </summary>
    public int SystemBytes { get; set; }

    /// <summary>
    /// 获取消息头。
    /// </summary>
    public HsmsHeader Header => new HsmsHeader
    {
        DeviceId = this.DeviceId,
        S = this.S,
        F = this.F,
        ReplyExpected = this.ReplyExpected,
        PType = this.PType,
        MessageType = this.MessageType,
        SystemBytes = this.SystemBytes
    };

    /// <summary>
    /// 获取或设置 SECS-II 数据体。
    /// </summary>
    public SecsItem? Body { get; set; }

    /// <inheritdoc/>
    public int MaxLength => 1024 * 64;

    int IWaitHandle.Sign
    {
        get => this.SystemBytes;
        set => this.SystemBytes = value;
    }

    #endregion

    #region 工厂方法

    /// <summary>
    /// 创建一条 Select.req 消息。
    /// </summary>
    /// <remarks>根据 SEMI E37 规范，控制消息的 Device ID 固定为 <see langword="0xFFFF"/>。</remarks>
    /// https://gitee.com/RRQM_Home/TouchSocket/issues/IJMFEC
    public static HsmsMessage CreateSelectRequest()
    {
        return new HsmsMessage
        {
            DeviceId = 0xFFFF,
            MessageType = HsmsMessageType.SelectRequest,
            ReplyExpected = true
        };
    }

    /// <summary>
    /// 创建一条 Select.rsp 消息。
    /// </summary>
    /// <param name="systemBytes">对应请求消息的系统字节。</param>
    /// <param name="status">选择状态码。</param>
    /// <remarks>根据 SEMI E37 规范，控制消息的 Device ID 固定为 <see langword="0xFFFF"/>。</remarks>
    public static HsmsMessage CreateSelectResponse(int systemBytes, SelectStatus status = SelectStatus.Success)
    {
        return new HsmsMessage
        {
            DeviceId = 0xFFFF,
            MessageType = HsmsMessageType.SelectResponse,
            SystemBytes = systemBytes,
            F = (byte)status
        };
    }

    /// <summary>
    /// 创建一条 Linktest.req 消息。
    /// </summary>
    /// <remarks>根据 SEMI E37 规范，控制消息的 Device ID 固定为 <see langword="0xFFFF"/>。</remarks>
    public static HsmsMessage CreateLinkTestRequest()
    {
        return new HsmsMessage
        {
            DeviceId = 0xFFFF,
            MessageType = HsmsMessageType.LinkTestRequest,
            ReplyExpected = true
        };
    }

    /// <summary>
    /// 创建一条 Linktest.rsp 消息。
    /// </summary>
    /// <param name="systemBytes">对应请求消息的系统字节。</param>
    /// <remarks>根据 SEMI E37 规范，控制消息的 Device ID 固定为 <see langword="0xFFFF"/>。</remarks>
    public static HsmsMessage CreateLinkTestResponse(int systemBytes)
    {
        return new HsmsMessage
        {
            DeviceId = 0xFFFF,
            MessageType = HsmsMessageType.LinkTestResponse,
            SystemBytes = systemBytes
        };
    }

    /// <summary>
    /// 创建一条 Separate.req 消息。
    /// </summary>
    /// <remarks>根据 SEMI E37 规范，控制消息的 Device ID 固定为 <see langword="0xFFFF"/>。</remarks>
    public static HsmsMessage CreateSeparateRequest()
    {
        return new HsmsMessage
        {
            DeviceId = 0xFFFF,
            MessageType = HsmsMessageType.SeparateRequest
        };
    }

    #endregion

    #region 序列化

    /// <inheritdoc/>
    public void Build<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        var anchor = new WriterAnchor<TWriter>(ref writer, 4);

        HsmsHeader.Write(ref writer, this.Header);

        this.Body?.Package(ref writer);

        var lengthSpan = anchor.Rewind(ref writer, out var bodyLength);
        TouchSocketBitConverter.BigEndian.WriteBytes(lengthSpan, (uint)bodyLength);
    }

    /// <summary>
    /// 从读取器中解析 <see cref="HsmsMessage"/>。
    /// </summary>
    /// <typeparam name="TReader">读取器类型。</typeparam>
    /// <param name="reader">读取器。</param>
    public void Unpackage<TReader>(ref TReader reader) where TReader : IBytesReader
    {
        var header = HsmsHeader.Read(ref reader);

        this.DeviceId = header.DeviceId;
        this.S = header.S;
        this.F = header.F;
        this.ReplyExpected = header.ReplyExpected;
        this.PType = header.PType;
        this.MessageType = header.MessageType;
        this.SystemBytes = header.SystemBytes;

        if (reader.BytesRemaining > 0)
        {
            this.Body = SecsItem.ReadSecsItem(ref reader);
        }
    }

    #endregion
}
