using System;
using System.Threading;
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

        /// <summary>
        /// 连接到指定的命名管道
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="pipeName">管道名称</param>
        /// <param name="timeout">超时设定</param>
        /// <returns></returns>
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
            client.Connect(timeout,CancellationToken.None);
            return client;
        }

        /// <summary>
        /// 异步连接到指定的命名管道
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="pipeName">管道名称</param>
        /// <param name="timeout">超时设定</param>
        /// <returns></returns>
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
            await client.ConnectAsync(timeout,CancellationToken.None);
            return client;
        }
        #endregion 连接
    }
}