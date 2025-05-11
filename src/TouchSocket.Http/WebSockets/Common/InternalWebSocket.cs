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

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

internal sealed partial class InternalWebSocket : IWebSocket
{
    private readonly HttpClientBase m_httpClientBase;
    private readonly HttpSessionClient m_httpSocketClient;
    private readonly bool m_isServer;
    private bool m_allowAsyncRead;
    private bool m_isCont;

    public InternalWebSocket(HttpClientBase httpClientBase) : base(true)
    {
        this.m_isServer = false;
        this.m_httpClientBase = httpClientBase;
    }

    public InternalWebSocket(HttpSessionClient httpSocketClient) : base(true)
    {
        this.m_isServer = true;
        this.m_httpSocketClient = httpSocketClient;
    }

    public bool AllowAsyncRead { get => this.m_allowAsyncRead; set => this.m_allowAsyncRead = value; }
    public IHttpSession Client => this.m_isServer ? this.m_httpSocketClient : this.m_httpClientBase;
    public WebSocketCloseStatus CloseStatus { get; set; }
    public bool IsClient => !this.m_isServer;
    public DateTimeOffset LastReceivedTime => this.Client.LastReceivedTime;
    public DateTimeOffset LastSentTime => this.Client.LastSentTime;
    public ILog Logger => this.Client.Logger;
    public bool Online { get; set; }
    public Protocol Protocol => Protocol.WebSocket;
    public IResolver Resolver => this.m_isServer ? this.m_httpSocketClient.Resolver : this.m_httpClientBase.Resolver;
    public string Version { get; set; }

    public async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            return await this.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, token);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken token = default)
    {
        if (!this.Online)
        {
            return Result.Success;
        }

        this.CloseStatus = closeStatus;

        try
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close })
            {
                using (var byteBlock = new ByteBlock(1024))
                {
                    byteBlock.WriteUInt16((ushort)closeStatus, EndianType.Big);
                    if (statusDescription.HasValue())
                    {
                        byteBlock.WriteNormalString(statusDescription, Encoding.UTF8);
                    }
                    frame.PayloadData = byteBlock;
                    await this.SendAsync(frame).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            if (this.m_isServer)
            {
                await this.m_httpSocketClient.ShutdownAsync(System.Net.Sockets.SocketShutdown.Both);
                await this.m_httpSocketClient.CloseAsync(statusDescription, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                await this.m_httpClientBase.ShutdownAsync(System.Net.Sockets.SocketShutdown.Both);
                await this.m_httpClientBase.CloseAsync(statusDescription, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> PingAsync()
    {
        try
        {
            await this.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> PongAsync()
    {
        try
        {
            await this.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {

            return Result.FromException(ex);
        }
        
    }

    #region 发送

    public async Task SendAsync(string text, bool endOfMessage = true)
    {
        using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Text }.AppendText(text))
        {
            await this.SendAsync(frame, endOfMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    public async Task SendAsync(ReadOnlyMemory<byte> memory, bool endOfMessage = true)
    {
        using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
        {
            frame.AppendBinary(memory.Span);
            await this.SendAsync(frame, endOfMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    public async Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true)
    {
        WSDataType dataType;
        if (this.m_isCont)
        {
            dataType = WSDataType.Cont;
            if (endOfMessage)
            {
                this.m_isCont = false;
            }
        }
        else
        {
            dataType = dataFrame.Opcode;
            if (!endOfMessage)
            {
                this.m_isCont = true;
            }
        }
        dataFrame.Opcode = dataType;

        var byteBlock = new ByteBlock(dataFrame.MaxLength);
        try
        {
            if (this.m_isServer)
            {
                dataFrame.BuildResponse(ref byteBlock);
                await this.m_httpSocketClient.InternalSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                dataFrame.BuildRequest(ref byteBlock);
                await this.m_httpClientBase.InternalSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    #endregion 发送

    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        base.Dispose(disposing);

        if (disposing)
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
}