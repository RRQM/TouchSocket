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

using TouchSocket.Core;

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 消息基类（RFC 7252 Section 3）。
/// 包含消息头、Token、选项和有效载荷的序列化与反序列化逻辑。
/// </summary>
public abstract class CoAPMessage : IRequestInfo
{
    /// <summary>
    /// CoAP 协议版本，固定为 1。
    /// </summary>
    public const byte Version = 1;

    /// <summary>
    /// 有效载荷标记字节。
    /// </summary>
    private const byte PayloadMarker = 0xFF;

    /// <summary>
    /// 获取或设置消息类型。
    /// </summary>
    public CoAPMessageType Type { get; set; }

    /// <summary>
    /// 获取原始消息码字节（Code 字段）。
    /// 请求使用 <see cref="CoAPMethod"/>，响应使用 <see cref="CoAPResponseCode"/>。
    /// </summary>
    public byte Code { get; protected set; }

    /// <summary>
    /// 获取或设置消息 ID（16 位，大端存储）。
    /// </summary>
    public ushort MessageId { get; set; }

    /// <summary>
    /// 获取或设置 Token（0-8 字节）。
    /// </summary>
    public ReadOnlyMemory<byte> Token { get; set; }

    /// <summary>
    /// 获取或设置消息的选项集合。
    /// </summary>
    public CoAPOptions Options { get; set; } = new CoAPOptions();

    /// <summary>
    /// 获取或设置有效载荷数据。
    /// </summary>
    public ReadOnlyMemory<byte> Payload { get; set; }

    /// <summary>
    /// 获取消息码的 3 位类别（高 3 位）。
    /// 0 = 请求，2 = 成功，4 = 客户端错误，5 = 服务器错误。
    /// </summary>
    public int CodeClass => (this.Code >> 5) & 0x07;

    /// <summary>
    /// 获取消息码的 5 位细节（低 5 位）。
    /// </summary>
    public int CodeDetail => this.Code & 0x1F;

    /// <summary>
    /// 获取一个值，指示该消息是否为请求消息（<see cref="CodeClass"/> == 0）。
    /// </summary>
    public bool IsRequest => this.CodeClass == 0;

    /// <summary>
    /// 获取一个值，指示该消息是否为响应消息（<see cref="CodeClass"/> > 0）。
    /// </summary>
    public bool IsResponse => this.CodeClass > 0;

    /// <summary>
    /// 将消息序列化并写入 <see cref="IBytesWriter"/>。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IBytesWriter"/> 接口的写入器类型。</typeparam>
    /// <param name="writer">目标写入器。</param>
    public void Build<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        var tokenLen = this.Token.Length;
        Span<byte> header = stackalloc byte[4];
        header[0] = (byte)((Version << 6) | ((int)this.Type << 4) | (tokenLen & 0x0F));
        header[1] = this.Code;
        header[2] = (byte)(this.MessageId >> 8);
        header[3] = (byte)this.MessageId;
        writer.Write(header);
        writer.Write(this.Token.Span);
        this.Options.Encode(ref writer);
        if (this.Payload.Length > 0)
        {
            Span<byte> marker = stackalloc byte[] { PayloadMarker };
            writer.Write(marker);
            writer.Write(this.Payload.Span);
        }
    }

    /// <summary>
    /// 从字节数组解析 CoAP 消息（工厂方法）。
    /// 根据 Code 字段自动返回 <see cref="CoAPRequest"/> 或 <see cref="CoAPResponse"/>。
    /// </summary>
    /// <param name="data">原始字节数据。</param>
    /// <returns>解析后的 <see cref="CoAPMessage"/>，请求返回 <see cref="CoAPRequest"/>，响应返回 <see cref="CoAPResponse"/>。</returns>
    /// <exception cref="CoAPException">数据长度不足或格式不合法时抛出。</exception>
    public static CoAPMessage Parse(ReadOnlySpan<byte> data)
    {
        CoAPThrowHelper.ThrowIfInvalidCoAPMessageLength(data.Length);

        var first = data[0];
        var ver = (first >> 6) & 0x03;
        var type = (CoAPMessageType)((first >> 4) & 0x03);
        var tkl = first & 0x0F;

        CoAPThrowHelper.ThrowIfInvalidCoAPVersion(ver);
        CoAPThrowHelper.ThrowIfInvalidTokenLength(tkl);

        var code = data[1];
        var msgId = (ushort)((data[2] << 8) | data[3]);

        var pos = 4;
        CoAPThrowHelper.ThrowIfDataTooShort(data.Length, pos + tkl);

        var token = data.Slice(pos, tkl).ToArray();
        pos += tkl;

        var optionsAndPayload = data.Slice(pos);
        var payloadMarkerIdx = FindPayloadMarker(optionsAndPayload);

        byte[] optionBytes;
        byte[] payload;

        if (payloadMarkerIdx < 0)
        {
            optionBytes = optionsAndPayload.ToArray();
            payload = Array.Empty<byte>();
        }
        else
        {
            optionBytes = optionsAndPayload.Slice(0, payloadMarkerIdx).ToArray();
            payload = optionsAndPayload.Slice(payloadMarkerIdx + 1).ToArray();
        }

        var options = CoAPOptions.Decode(optionBytes, 0, optionBytes.Length);

        var codeClass = (code >> 5) & 0x07;

        CoAPMessage message;
        if (codeClass == 0)
        {
            var request = new CoAPRequest
            {
                Type = type,
                Code = code,
                MessageId = msgId,
                Token = token,
                Payload = payload,
                Options = options,
            };
            message = request;
        }
        else
        {
            var response = new CoAPResponse
            {
                Type = type,
                Code = code,
                MessageId = msgId,
                Token = token,
                Payload = payload,
                Options = options,
            };
            message = response;
        }

        return message;
    }

    private static int FindPayloadMarker(ReadOnlySpan<byte> data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] == PayloadMarker)
            {
                return i;
            }
        }

        return -1;
    }
}
