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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// HttpClient客户端连接池
    /// </summary>
    public class HttpClientPool : SetupConfigObject
    {
        private readonly ConcurrentStack<HttpClient> m_httpClients = new ConcurrentStack<HttpClient>();

        /// <summary>
        /// 最大连接数量。
        /// </summary>
        public int MaxCount { get; set; } = 10;

        /// <summary>
        /// 目标地址
        /// </summary>
        public IPHost RemoteIPHost { get; private set; }

        /// <summary>
        /// 清除现有的所有链接
        /// </summary>
        public void Clear()
        {
            while (this.m_httpClients.TryPop(out var client))
            {
                client.SafeDispose();
            }
        }

        /// <summary>
        /// 发起请求，并获取数据体
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="millisecondsTimeout">等待超时时间</param>
        /// <param name="token">结束等待令箭</param>
        /// <returns></returns>
        public HttpResponse RequestContent(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            var client = this.GetHttpClient();
            try
            {
                return client.RequestContent(request, false, millisecondsTimeout, token);
            }
            finally
            {
                this.ReturnHttpClient(client);
            }
        }

        /// <summary>
        /// 发起请求，并获取数据体
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="millisecondsTimeout">等待超时时间</param>
        /// <param name="token">结束等待令箭</param>
        /// <returns></returns>
        public async Task<HttpResponse> RequestContentAsync(HttpRequest request, int millisecondsTimeout = 10000, CancellationToken token = default)
        {
            var client = await this.GetHttpClientAsync();
            try
            {
                return await client.RequestContentAsync(request, false, millisecondsTimeout, token);
            }
            finally
            {
                this.ReturnHttpClient(client);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty) ?? throw new ArgumentNullException(nameof(this.RemoteIPHost));
            base.LoadConfig(config);
        }

        private HttpClient GetHttpClient()
        {
            this.ThrowIfDisposed();
            HttpClient client;
            while (this.m_httpClients.TryPop(out client))
            {
                if (client.Online)
                {
                    return client;
                }
                else
                {
                    client.SafeDispose();
                }
            }

            client = new HttpClient();
            client.Setup(this.Config.Clone());
            client.Connect();
            return client;
        }

        private async Task<HttpClient> GetHttpClientAsync()
        {
            this.ThrowIfDisposed();
            HttpClient client;
            while (this.m_httpClients.TryPop(out client))
            {
                if (client.Online)
                {
                    return client;
                }
                else
                {
                    client.SafeDispose();
                }
            }

            client = new HttpClient();
            client.Setup(this.Config.Clone());
            await client.ConnectAsync();
            return client;
        }

        private void ReturnHttpClient(HttpClient client)
        {
            if (!client.Online || this.m_httpClients.Count >= 10)
            {
                client.SafeDispose();
                return;
            }
            this.m_httpClients.Push(client);
        }
    }
}