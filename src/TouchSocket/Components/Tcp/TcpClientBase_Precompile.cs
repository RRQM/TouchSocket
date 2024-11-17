﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    public partial class TcpClientBase
    {
        #region Connect


#if NET6_0_OR_GREATER

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <param name="millisecondsTimeout">连接超时时间，单位为毫秒</param>
        /// <param name="token">用于取消操作的令牌</param>
        /// <returns>返回一个异步任务</returns>
        /// <exception cref="ObjectDisposedException">如果对象已被处置，则抛出此异常</exception>
        /// <exception cref="ArgumentNullException">如果必要参数为空，则抛出此异常</exception>
        /// <exception cref="TimeoutException">如果连接超时，则抛出此异常</exception>
        protected async Task TcpConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            // 检查对象是否已被处置
            this.ThrowIfDisposed();
            // 检查配置是否为空
            this.ThrowIfConfigIsNull();
            // 等待信号量，以控制并发连接
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);
            try
            {
                // 如果已经在线，则无需再次连接
                if (this.m_online)
                {
                    return;
                }

                // 确保远程IP和端口已设置
                var iPHost = ThrowHelper.ThrowArgumentNullExceptionIf(this.RemoteIPHost, nameof(this.RemoteIPHost));
                // 释放之前的Socket资源
                this.MainSocket.SafeDispose();
                // 创建新的Socket连接
                var socket = this.CreateSocket(iPHost);
                // 触发连接前的事件
                var args = new ConnectingEventArgs();
                await this.PrivateOnTcpConnecting(args).ConfigureAwait(false);
                // 根据取消令牌决定是否支持异步取消
                if (token.CanBeCanceled)
                {
                    await socket.ConnectAsync(iPHost.Host, iPHost.Port, token).ConfigureAwait(false);
                }
                else
                {
                    // 使用信号量控制超时
                    using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
                    {
                        try
                        {
                            // 尝试连接
                            await socket.ConnectAsync(iPHost.Host, iPHost.Port, tokenSource.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            // 超时处理
                            throw new TimeoutException();
                        }
                    }
                }
                // 更新在线状态
                this.m_online = true;
                // 设置当前Socket
                this.SetSocket(socket);
                // 进行身份验证
                await this.AuthenticateAsync().ConfigureAwait(false);
                // 触发连接成功的事件
                _ = Task.Factory.StartNew(this.PrivateOnTcpConnected, new ConnectedEventArgs());
            }
            finally
            {
                // 释放信号量
                this.m_semaphoreForConnect.Release();
            }
        }
#else

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <param name="millisecondsTimeout">连接超时时间，单位为毫秒</param>
        /// <param name="token">取消令牌</param>
        /// <returns>返回任务</returns>
        protected async Task TcpConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            // 检查对象是否已释放
            this.ThrowIfDisposed();
            // 检查配置是否为空
            this.ThrowIfConfigIsNull();
            // 等待信号量，以确保同时只有一个连接操作
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);

            try
            {
                // 如果已经在线，则无需进行连接操作
                if (this.m_online)
                {
                    return;
                }
                // 获取远程IP和端口信息
                var iPHost = ThrowHelper.ThrowArgumentNullExceptionIf(this.RemoteIPHost, nameof(this.RemoteIPHost));
                // 释放之前的Socket资源
                this.MainSocket.SafeDispose();
                // 创建新的Socket连接
                var socket = this.CreateSocket(iPHost);
                // 触发连接前的事件
                await this.PrivateOnTcpConnecting(new ConnectingEventArgs()).ConfigureAwait(false);
                // 异步开始连接
                var task = Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, iPHost.Host, iPHost.Port, null);
                // 等待连接完成，设置超时时间
                await task.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout)).ConfigureAwait(false);
                // 更新在线状态
                this.m_online = true;
                // 设置新的Socket为当前使用
                this.SetSocket(socket);
                // 进行身份验证
                await this.AuthenticateAsync().ConfigureAwait(false);
                // 启动新任务，处理连接后的操作
                _ = Task.Factory.StartNew(this.PrivateOnTcpConnected, new ConnectedEventArgs());
            }
            finally
            {
                // 释放信号量，允许其他连接操作
                this.m_semaphoreForConnect.Release();
            }
        }

#endif

        #endregion Connect


        private Socket CreateSocket(IPHost iPHost)
        {
            Socket socket;
            if (iPHost.HostNameType == UriHostNameType.Dns)
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };
            }
            else
            {
                socket = new Socket(iPHost.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.KeepAliveValueProperty) is KeepAliveValue keepAliveValue)
            {
#if NET45_OR_GREATER

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
#else
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
                }
#endif
            }

            var noDelay = this.Config.GetValue(TouchSocketConfigExtension.NoDelayProperty);
            if (noDelay.HasValue)
            {
                socket.NoDelay = noDelay.Value;
            }

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
}
