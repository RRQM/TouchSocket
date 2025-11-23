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

using System.Buffers;
using System.Net.Sockets;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// Tcp核心
/// </summary>
internal sealed class TcpCore : SafetyDisposableObject
{
    /// <summary>
    /// 析构函数
    /// </summary>
    ~TcpCore()
    {
        this.Dispose(disposing: false);
    }

    #region 字段

    //private readonly SocketReceiver2 m_socketReceiver = new(System.IO.Pipelines.PipeScheduler.ThreadPool);
    private readonly SocketReceiver m_socketReceiver = new();
    private readonly SocketSender m_socketSender = new();
    private Socket m_socket;

    #endregion 字段

    public bool ReceiveRunContinuationsAsynchronously { get => this.m_socketReceiver.RunContinuationsAsynchronously; set => this.m_socketReceiver.RunContinuationsAsynchronously = value; }
    public bool SendRunContinuationsAsynchronously { get => this.m_socketSender.RunContinuationsAsynchronously; set => this.m_socketSender.RunContinuationsAsynchronously = value; }
    public Socket Socket => this.m_socket;


    #region Receive

    public ValueTask<TcpOperationResult> ReceiveAsync(in Memory<byte> memory)
    {
        return this.m_socketReceiver.ReceiveAsync(this.m_socket, memory);
    }

    public ValueTask<TcpOperationResult> WaitForDataAsync()
    {
        return this.m_socketReceiver.WaitForDataAsync(this.m_socket);
    }

    #endregion Receive

    /// <summary>
    /// 重置环境，并设置新的<see cref="Socket"/>。
    /// </summary>
    /// <param name="socket"></param>
    public void Reset(Socket socket)
    {
        ThrowHelper.ThrowIfNull(socket, nameof(socket));

        if (!socket.Connected)
        {
            ThrowHelper.ThrowException(TouchSocketResource.SocketHaveToConnected);
        }
        this.Reset();
        this.m_socket = socket;
    }

    /// <summary>
    /// 重置环境。
    /// </summary>
    public void Reset()
    {
        this.m_socket = null;
    }

    public async Task SendAsync(ReadOnlySequence<byte> buffer)
    {
        var length = buffer.Length;
        try
        {
            var result = await this.m_socketSender.SendAsync(this.m_socket, buffer).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (result.SocketError != SocketError.Success)
            {
                ThrowHelper.ThrowSocketException((int)result.SocketError);
            }

            if (result.BytesTransferred != length)
            {
                ThrowHelper.ThrowException(TouchSocketResource.IncompleteDataTransmission);
            }
        }
        finally
        {
            this.m_socketSender.Reset();
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_socketReceiver.SafeDispose();
            this.m_socketSender.SafeDispose();
        }
    }
}