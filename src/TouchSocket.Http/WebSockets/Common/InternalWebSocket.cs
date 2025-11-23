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

internal sealed partial class InternalWebSocket : IWebSocket
{
    private readonly HttpClientBase m_httpClientBase;
    private readonly HttpSessionClient m_httpSocketClient;
    private readonly bool m_isServer;
    private bool m_allowAsyncRead;
    private bool m_isCont;

    public InternalWebSocket(HttpClientBase httpClientBase)
    {
        this.m_isServer = false;
        this.m_httpClientBase = httpClientBase;
    }

    public InternalWebSocket(HttpSessionClient httpSocketClient)
    {
        this.m_isServer = true;
        this.m_httpSocketClient = httpSocketClient;
    }

    public bool AllowAsyncRead { get => this.m_allowAsyncRead; set => this.m_allowAsyncRead = value; }
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

    public async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            return await this.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
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
                await this.SendAsync(frame, true, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }

            if (this.m_isServer)
            {
                await this.m_httpSocketClient.CloseAsync(statusDescription, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                await this.m_httpClientBase.CloseAsync(statusDescription, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await this.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping }, cancellationToken: cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> PongAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await this.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong }, cancellationToken: cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    #region 发送

    public async Task SendAsync(string text, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(text, nameof(text));
        var byteBlock = new ByteBlock(Encoding.UTF8.GetMaxByteCount(text.Length));
        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, text, Encoding.UTF8);
            var frame = new WSDataFrame(byteBlock.Memory) { FIN = endOfMessage, Opcode = WSDataType.Text };
            await this.SendAsync(frame, endOfMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    public async Task SendAsync(ReadOnlyMemory<byte> memory, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        var frame = new WSDataFrame(memory) { FIN = endOfMessage, Opcode = WSDataType.Binary };
        await this.SendAsync(frame, endOfMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

        var transport = this.m_isServer ? this.m_httpSocketClient.InternalTransport : this.m_httpClientBase.InternalTransport;

        await transport.WriteLocker.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            var writer = new PipeBytesWriter(transport.Writer);

            dataFrame.Build(ref writer);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            transport.WriteLocker.Release();
        }
    }

    #endregion 发送

    protected override void SafetyDispose(bool disposing)
    {
        if (this.m_isServer)
        {
            this.m_httpSocketClient.SafeDispose();
        }
        else
        {
            this.m_httpClientBase.SafeDispose();
        }
    }
}