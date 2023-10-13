using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// NamedPipeClientExtension
    /// </summary>
    public static class NamedPipeClientExtension
    {
        #region 连接

        /// <inheritdoc cref="INamedPipeClient.Connect(int)"/>
        public static TClient Connect<TClient>(this TClient client, string pipeName, int timeout = 5000) where TClient : INamedPipeClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetPipeName(pipeName);
                client.Setup(config);
            }
            else
            {
                config = client.Config;
                config.SetPipeName(pipeName);
            }
            client.Connect(timeout);
            return client;
        }

        /// <inheritdoc cref="INamedPipeClient.ConnectAsync(int)"/>
        public static async Task<TClient> ConnectAsync<TClient>(this TClient client, string pipeName, int timeout = 5000) where TClient : INamedPipeClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetPipeName(pipeName);
                client.Setup(config);
            }
            else
            {
                config = client.Config;
                config.SetPipeName(pipeName);
            }
            await client.ConnectAsync(timeout);
            return client;
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Result TryConnect<TClient>(this TClient client, int timeout = 5000) where TClient : INamedPipeClient
        {
            try
            {
                client.Connect(timeout);
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
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<Result> TryConnectAsync<TClient>(this TClient client, int timeout = 5000) where TClient : INamedPipeClient
        {
            try
            {
                await client.ConnectAsync(timeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        #endregion 连接
    }
}
