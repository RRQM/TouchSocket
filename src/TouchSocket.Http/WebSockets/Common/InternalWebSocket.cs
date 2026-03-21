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

using System.Net.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

internal sealed partial class InternalWebSocket : SafetyDisposableObject, IWebSocket
{
    private readonly bool m_autoReceive;
    private readonly HttpClientBase m_httpClientBase;
    private readonly HttpSessionClient m_httpSocketClient;
    private readonly bool m_isServer;
    private readonly ITransport m_transport;
    private bool m_isCont;

    public InternalWebSocket(HttpClientBase httpClientBase, ITransport transport, bool autoReceive)
    {
        this.m_isServer = false;
        this.m_httpClientBase = httpClientBase;
        this.m_transport = transport;
        this.m_autoReceive = autoReceive;
    }

    public InternalWebSocket(HttpSessionClient httpSocketClient, ITransport transport, bool autoReceive)
    {
        this.m_isServer = true;
        this.m_httpSocketClient = httpSocketClient;
        this.m_transport = transport;
        this.m_autoReceive = autoReceive;
    }

    public IHttpSession Client => this.m_isServer ? this.m_httpSocketClient : this.m_httpClientBase;
    public CancellationToken ClosedToken => this.m_isServer ? this.m_httpSocketClient.ClosedToken : this.m_httpClientBase.ClosedToken;
    public WebSocketCloseStatus CloseStatus { get; set; }
    public bool IsClient => !this.m_isServer;
    public DateTimeOffset LastReceivedTime => this.Client.LastReceivedTime;
    public DateTimeOffset LastSentTime => this.Client.LastSentTime;
    public ILog Logger => this.Client.Logger;
    public bool Online { get; set; }
    public Protocol Protocol => Protocol.WebSocket;
    public IResolver Resolver => this.m_isServer ? this.m_httpSocketClient.Resolver : this.m_httpClientBase.Resolver;
    public string Version { get; set; }

    public Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        return this.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, cancellationToken);
    }

    public async Task<Result> CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default)
    {
        if (!this.Online)
        {
            return Result.Success;
        }

        this.CloseStatus = closeStatus;

        try
        {
            var byteBlock = new ByteBlock(1024);
            try
            {
                WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, (ushort)closeStatus, EndianType.Big);
                if (statusDescription.HasValue())
                {
                    WriterExtension.WriteNormalString(ref byteBlock, statusDescription, Encoding.UTF8);
                }
                var frame = new WSDataFrame(byteBlock.Memory) { FIN = true, Opcode = WSDataType.Close };
                await this.SendAsync(frame, true, cancellationToken).ConfigureDefaultAwait();
            }
            finally
            {
                byteBlock.Dispose();
            }

            //防止关闭递归死锁
            _ = EasyTask.SafeNewRun(async () =>
            {
                if (this.m_isServer)
                {
                    await this.m_httpSocketClient.CloseAsync(statusDescription, cancellationToken).ConfigureDefaultAwait();
                }
                else
                {
                    await this.m_httpClientBase.CloseAsync(statusDescription, cancellationToken).ConfigureDefaultAwait();
                }
            });

            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async ValueTask<WebSocketReceiveResult> InternalReadAsync(CancellationToken cancellationToken)
    {
        var transport = this.m_transport;

        var pipeReader = transport.Reader;

        while (!this.DisposedValue)
        {
            if (!this.Online)
            {
                return new WebSocketReceiveResult(null, null, true);
            }

            var frame = new WSDataFrame();
            IBigUnfixedHeaderRequestInfo frameInfo = frame;

            // 阶段一：解析帧头部
            var headerParsed = false;
            while (!headerParsed)
            {
                var readResult = await pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
                var buffer = readResult.Buffer;
                if (readResult.IsCanceled || (readResult.IsCompleted && buffer.Length == 0))
                {
                    frame.Dispose();
                    return new WebSocketReceiveResult(null, null, true);
                }

                var bytesReader = new BytesReader(buffer);
                bool parsed;
                long headerBytesRead;
                try
                {
                    parsed = frameInfo.OnParsingHeader(ref bytesReader);
                    headerBytesRead = bytesReader.BytesRead;
                }
                finally
                {
                    bytesReader.Dispose();
                }

                if (parsed)
                {
                    pipeReader.AdvanceTo(buffer.GetPosition(headerBytesRead));
                    headerParsed = true;
                }
                else
                {
                    if (readResult.IsCompleted)
                    {
                        frame.Dispose();
                        return new WebSocketReceiveResult(null, null, true);
                    }
                    pipeReader.AdvanceTo(buffer.Start, buffer.End);
                }
            }

            // 阶段二：读取 payload
            var remaining = frameInfo.BodyLength;
            while (remaining > 0)
            {
                var readResult = await pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
                var buffer = readResult.Buffer;
                if (readResult.IsCanceled || (readResult.IsCompleted && buffer.Length == 0))
                {
                    frame.Dispose();
                    return new WebSocketReceiveResult(null, null, true);
                }

                var toRead = Math.Min(remaining, buffer.Length);
                foreach (var segment in buffer.Slice(0, toRead))
                {
                    frameInfo.OnAppendBody(segment.Span);
                }
                remaining -= toRead;
                pipeReader.AdvanceTo(buffer.GetPosition(toRead));
            }

            // 阶段三：完成帧
            if (!frameInfo.OnFinished())
            {
                frame.Dispose();
                continue;
            }

            // 处理控制帧
            if (frame.IsClose)
            {
                var payloadSpan = frame.PayloadData.Span;
                if (payloadSpan.Length >= 2)
                {
                    this.CloseStatus = (WebSocketCloseStatus)payloadSpan.ReadValue<ushort>(EndianType.Big);
                }
                var msg = payloadSpan.ToString(Encoding.UTF8);
                return new WebSocketReceiveResult(null, msg, true);
            }

            return new WebSocketReceiveResult(frame, null, false);
        }

        return new WebSocketReceiveResult(null, null, true);
    }

    /// <inheritdoc/>
    public ValueTask<WebSocketReceiveResult> ReadAsync(CancellationToken cancellationToken)
    {
        if (this.m_autoReceive)
        {
            ThrowHelper.ThrowInvalidOperationException("在自动接收模式下不能调用ReadAsync");
        }

        return this.InternalReadAsync(cancellationToken);
    }

    public async Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        var opcode = dataFrame.Opcode;
        var isControl = opcode == WSDataType.Close || opcode == WSDataType.Ping || opcode == WSDataType.Pong;

        if (isControl)
        {
            // 控制帧不能分片，必须FIN=true，且不影响当前分片状态
            dataFrame.FIN = true;
            dataFrame.Opcode = opcode;
        }
        else
        {
            // 文本/二进制数据帧按分片状态机处理
            dataFrame.FIN = endOfMessage;
            if (this.m_isCont)
            {
                dataFrame.Opcode = WSDataType.Cont;
                if (endOfMessage)
                {
                    this.m_isCont = false;
                }
            }
            else
            {
                dataFrame.Opcode = opcode;
                if (!endOfMessage)
                {
                    this.m_isCont = true;
                }
            }
        }

        dataFrame.Mask = !this.m_isServer;

        var transport = this.m_transport;

        await transport.WriteLocker.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var writer = new PipeBytesWriter(transport.Writer);

            dataFrame.Build(ref writer);
            await writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            transport.WriteLocker.Release();
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (this.m_isServer)
        {
            this.m_httpSocketClient.Dispose();
        }
        else
        {
            this.m_httpClientBase.Dispose();
        }
    }
}