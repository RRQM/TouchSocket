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
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 客户端扩展类
    /// </summary>
    public static class ClientExtension
    {
        /// <summary>
        /// 获取相关信息。格式：
        ///<para>IPPort=IP:Port,Id=id,Protocol=Protocol</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetInfo<T>(this T client) where T : ITcpSession, IIdClient
        {
            return $"IP&Port={client.IP}:{client.Port},Id={client.Id},Protocol={client.Protocol}";
        }

        /// <summary>
        /// 获取IP和端口。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetIPPort<T>(this T client) where T : ITcpSession
        {
            return $"{client.IP}:{client.Port}";
        }

        /// <summary>
        /// 获取最后活动时间。即<see cref="IClient.LastReceivedTime"/>与<see cref="IClient.LastSentTime"/>的最近值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static DateTime GetLastActiveTime<T>(this T client) where T : IClient
        {
            return client.LastSentTime > client.LastReceivedTime ? client.LastSentTime : client.LastReceivedTime;
        }

        /// <summary>
        /// 获取服务器中，除自身以外的所有客户端id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetOtherIds<T>(this T client) where T : ITcpListenableClient, IIdClient
        {
            return client.Service.GetIds().Where(id => id != client.Id);
        }

        /// <summary>
        /// 安全性发送关闭报文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="how"></param>
        public static bool TryShutdown<T>(this T client, SocketShutdown how = SocketShutdown.Both) where T : class, ITcpSession
        {
            if (client == default(T))
            {
                return false;
            }
            try
            {
                if (!client.MainSocket.Connected)
                {
                    return false;
                }
                client?.MainSocket?.Shutdown(how);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region CloseAsync

        /// <summary>
        /// <inheritdoc cref="IClosableClient.CloseAsync(string)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        public static Task CloseAsync<T>(this T client) where T : IClosableClient
        {
            return client.CloseAsync(string.Empty);
        }

        /// <summary>
        /// 安全性关闭。不会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static async Task SafeCloseAsync<T>(this T client, string msg) where T : IClosableClient
        {
            try
            {
                if (client == null)
                {
                    return;
                }
                else
                {
                    await client.CloseAsync(msg).ConfigureFalseAwait();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        ///  安全性关闭。不会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        public static Task SafeCloseAsync<T>(this T client) where T : IClosableClient
        {
            return SafeCloseAsync(client, nameof(SafeCloseAsync));
        }

        #endregion CloseAsync

        #region Close

        /// <summary>
        /// 同步关闭客户端
        /// </summary>
        /// <remarks>
        /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        public static void Close<T>(this T client) where T : IClosableClient
        {
            client.CloseAsync(string.Empty).GetFalseAwaitResult();
        }

        /// <summary>
        /// 同步关闭客户端
        /// </summary>
        /// <remarks>
        /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static void Close<T>(this T client, string msg) where T : IClosableClient
        {
            client.CloseAsync(msg).GetFalseAwaitResult();
        }

        /// <summary>
        /// 安全性关闭。不会抛出异常。
        /// </summary>
        /// <remarks>
        /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static void SafeClose<T>(this T client, string msg) where T : IClosableClient
        {
            try
            {
                if (client == null)
                {
                    return;
                }
                else
                {
                    client.CloseAsync(msg).GetFalseAwaitResult();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        ///  安全性关闭。不会抛出异常。
        /// </summary>
        /// <remarks>
        /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        public static void SafeClose<T>(this T client) where T : IClosableClient
        {
            SafeClose(client, nameof(SafeClose));
        }

        #endregion Close

        #region ConnectAsync

        /// <inheritdoc cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>
        public static async Task ConnectAsync(this IConnectableClient client, int millisecondsTimeout = 5000)
        {
            await client.ConnectAsync(millisecondsTimeout, CancellationToken.None).ConfigureFalseAwait();
        }

        /// <inheritdoc cref="IConnectableClient.ConnectAsync(int, System.Threading.CancellationToken)"/>
        public static async Task ConnectAsync<TClient>(this TClient client, IPHost ipHost, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetRemoteIPHost(ipHost);
                await client.SetupAsync(config).ConfigureFalseAwait();
            }
            else
            {
                config = client.Config;
                config.SetRemoteIPHost(ipHost);
            }
            await client.ConnectAsync(millisecondsTimeout).ConfigureFalseAwait();
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static async Task<Result> TryConnectAsync(this IConnectableClient client, int millisecondsTimeout = 5000)
        {
            try
            {
                await client.ConnectAsync(millisecondsTimeout).ConfigureFalseAwait();
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static async Task<Result> TryConnectAsync<TClient>(this TClient client, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
        {
            try
            {
                await client.ConnectAsync(millisecondsTimeout).ConfigureFalseAwait();
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        #endregion ConnectAsync

        #region Connect

        /// <summary>
        /// 同步执行连接操作。
        /// </summary>
        /// <remarks>
        /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
        /// </remarks>
        /// <param name="client"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="cancellationToken"></param>
        public static void Connect(this IConnectableClient client, int millisecondsTimeout = 5000, CancellationToken cancellationToken = default)
        {
            client.ConnectAsync(millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
        }

        /// <summary>
        /// 同步执行连接操作。
        /// </summary>
        /// <remarks>
        /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
        /// </remarks>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="ipHost"></param>
        /// <param name="millisecondsTimeout"></param>
        public static void Connect<TClient>(this TClient client, IPHost ipHost, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
        {
            ConnectAsync(client, ipHost, millisecondsTimeout).GetFalseAwaitResult();
        }

        /// <summary>
        /// 同步执行连接操作。不会抛出异常。
        /// </summary>
        /// <remarks>
        /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
        /// </remarks>
        /// <param name="client"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static Result TryConnect(this IConnectableClient client, int millisecondsTimeout = 5000)
        {
            try
            {
                client.ConnectAsync(millisecondsTimeout).GetFalseAwaitResult();
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 同步执行连接操作。不会抛出异常。
        /// </summary>
        /// <remarks>
        /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
        /// </remarks>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static Result TryConnect<TClient>(this TClient client, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
        {
            try
            {
                client.ConnectAsync(millisecondsTimeout).GetFalseAwaitResult();
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        #endregion Connect
    }
}