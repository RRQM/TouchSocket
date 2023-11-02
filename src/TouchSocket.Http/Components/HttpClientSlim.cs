#if !NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
    /// </summary>
    public class HttpClientSlim : SetupConfigObject
    {
        private readonly System.Net.Http.HttpClient m_httpClient;

        /// <summary>
        /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpClientSlim(System.Net.Http.HttpClient httpClient = default)
        {
            httpClient ??= new System.Net.Http.HttpClient();
            this.m_httpClient = httpClient;
        }

        /// <summary>
        /// 通讯客户端
        /// </summary>
        public System.Net.Http.HttpClient HttpClient => this.m_httpClient;

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.m_httpClient.BaseAddress ??= config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            base.LoadConfig(config);
        }
    }
}
#endif
