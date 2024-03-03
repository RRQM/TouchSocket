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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ConnectObjectExtension
    /// </summary>
    public static class ConnectObjectExtension
    {
        /// <inheritdoc cref="IConnectObject.Connect(int, CancellationToken)"/>
        public static void Connect(this IConnectObject client, int millisecondsTimeout = 5000)
        {
            client.Connect(millisecondsTimeout, CancellationToken.None);
        }

        /// <inheritdoc cref="IConnectObject.Connect(int, System.Threading.CancellationToken)"/>
        public static TClient Connect<TClient>(this TClient client, IPHost ipHost, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectObject
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetRemoteIPHost(ipHost);
                client.Setup(config);
            }
            else
            {
                config = client.Config;
                config.SetRemoteIPHost(ipHost);
            }
            client.Connect(millisecondsTimeout);
            return client;
        }

        /// <inheritdoc cref="IConnectObject.ConnectAsync(int, CancellationToken)"/>
        public static async Task ConnectAsync(this IConnectObject client, int millisecondsTimeout = 5000)
        {
            await client.ConnectAsync(millisecondsTimeout, CancellationToken.None);
        }

        /// <inheritdoc cref="IConnectObject.ConnectAsync(int, System.Threading.CancellationToken)"/>
        public static async Task<TClient> ConnectAsync<TClient>(this TClient client, IPHost ipHost, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectObject
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetRemoteIPHost(ipHost);
                await client.SetupAsync(config);
            }
            else
            {
                config = client.Config;
                config.SetRemoteIPHost(ipHost);
            }
            await client.ConnectAsync(millisecondsTimeout);
            return client;
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static Result TryConnect(this IConnectObject client, int millisecondsTimeout = 5000)
        {
            try
            {
                client.Connect(millisecondsTimeout);
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
        public static Result TryConnect<TClient>(this TClient client, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectObject
        {
            try
            {
                client.Connect(millisecondsTimeout);
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
        /// <param name="client"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static async Task<Result> TryConnectAsync(this IConnectObject client, int millisecondsTimeout = 5000)
        {
            try
            {
                await client.ConnectAsync(millisecondsTimeout);
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
        public static async Task<Result> TryConnectAsync<TClient>(this TClient client, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectObject
        {
            try
            {
                await client.ConnectAsync(millisecondsTimeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }
    }
}