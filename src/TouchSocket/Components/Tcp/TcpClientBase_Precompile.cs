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

using System.Net.Sockets;

namespace TouchSocket.Sockets;

public partial class TcpClientBase
{
    /// <summary>
    /// 异步连接服务器
    /// </summary>
    /// <param name="cancellationToken">用于取消操作的令牌</param>
    /// <returns>返回一个异步任务</returns>
    /// <exception cref="ObjectDisposedException">如果对象已被处置，则抛出此异常</exception>
    /// <exception cref="ArgumentNullException">如果必要参数为空，则抛出此异常</exception>
    /// <exception cref="TimeoutException">如果连接超时，则抛出此异常</exception>
    protected virtual async Task TcpConnectAsync(CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        this.ThrowIfConfigIsNull();

        await this.m_semaphoreForConnectAndClose.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            if (this.m_online)
            {
                return;
            }


            var iPHost = this.RemoteIPHost;
            ThrowHelper.ThrowIfNull(iPHost, nameof(this.RemoteIPHost));

            Socket socket = null;
            TcpTransport transport = null;

            try
            {
                socket = this.CreateSocket(iPHost);

                var args = new ConnectingEventArgs();
                await this.PrivateOnTcpConnecting(args).ConfigureDefaultAwait();

#if NET6_0_OR_GREATER
                await socket.ConnectAsync(iPHost.Host, iPHost.Port, cancellationToken).ConfigureDefaultAwait();
#else
                var task = Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, iPHost.Host, iPHost.Port, null);

                await task.WithCancellation(cancellationToken).ConfigureDefaultAwait();
#endif

                // 确保上次接收任务已经结束
                var runTask = this.m_runTask;
                if (runTask != null)
                {
                    await runTask.ConfigureDefaultAwait();
                }

                this.SetSocket(socket);

                transport = new TcpTransport(this.m_tcpCore, this.Config.GetValue(TouchSocketConfigExtension.TransportOptionProperty));
                this.m_transport = transport;
                await this.TryAuthenticateAsync(iPHost).ConfigureDefaultAwait();

                this.m_online = true;
                Interlocked.Exchange(ref this.m_closeFlag, 0);
                this.m_runTask = this.RunSessionAsync(transport);
            }
            catch
            {
                transport.SafeDispose();
                if (transport == null)
                {
                    socket.SafeDispose();
                }

                this.m_transport = default;
                this.m_online = false;
                this.SetSocket(null);
                throw;
            }
        }
        finally
        {
            this.m_semaphoreForConnectAndClose.Release();
        }
    }

    private Socket CreateSocket(IPHost iPHost)
    {
        Socket socket;
        if (iPHost.HostNameType == UriHostNameType.Dns)
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }
        else
        {
            socket = new Socket(iPHost.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        if (this.Config.GetValue(TouchSocketConfigExtension.KeepAliveValueProperty) is KeepAliveValue keepAliveValue)
        {
            keepAliveValue.Config(socket);
        }

        socket.NoDelay = this.Config.GetValue(TouchSocketConfigExtension.NoDelayProperty);

        if (this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty) != null)
        {
            if (this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty))
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            socket.Bind(this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty).EndPoint);
        }
        return socket;
    }
}