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

using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http客户端
    /// </summary>
    public class HttpClient : HttpClientBase, IHttpClient
    {
        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }

        ///// <inheritdoc/>
        //public HttpResponseResult Request(HttpRequest request, int millisecondsTimeout = 10000, CancellationToken token = default)
        //{
        //    return this.ProtectedRequest(request, millisecondsTimeout, token);
        //}

        /// <inheritdoc/>
        public Task<HttpResponseResult> RequestAsync(HttpRequest request, int millisecondsTimeout = 10000, CancellationToken token = default)
        {
            return this.ProtectedRequestAsync(request, millisecondsTimeout, token);
        }

        ///// <inheritdoc/>
        //public HttpResponseResult RequestContent(HttpRequest request, int millisecondsTimeout = 10000, CancellationToken token = default)
        //{
        //    return this.ProtectedRequestContent(request, millisecondsTimeout, token);
        //}

        /// <inheritdoc/>
        public Task<HttpResponseResult> RequestContentAsync(HttpRequest request, int millisecondsTimeout = 10000, CancellationToken token = default)
        {
            return this.ProtectedRequestContentAsync(request, millisecondsTimeout, token);
        }
    }
}