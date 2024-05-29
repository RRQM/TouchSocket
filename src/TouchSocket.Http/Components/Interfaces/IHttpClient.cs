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
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http终端接口
    /// </summary>
    public interface IHttpClient : IHttpSession, ISetupConfigObject, IOnlineClient, ITcpConnectableClient
    {
        ///// <summary>
        ///// 发起请求
        ///// </summary>
        ///// <param name="request">请求体</param>
        ///// <param name="millisecondsTimeout">等待超时时间</param>
        ///// <param name="token">结束等待令箭</param>
        ///// <returns></returns>
        //HttpResponseResult Request(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default);

        /// <summary>
        /// 发起请求
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="millisecondsTimeout">等待超时时间</param>
        /// <param name="token">结束等待令箭</param>
        /// <returns></returns>
        Task<HttpResponseResult> RequestAsync(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default);

        ///// <summary>
        ///// 发起请求，并获取数据体
        ///// </summary>
        ///// <param name="request">请求体</param>
        ///// <param name="millisecondsTimeout">等待超时时间</param>
        ///// <param name="token">结束等待令箭</param>
        ///// <returns></returns>
        //public HttpResponseResult RequestContent(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default);

        /// <summary>
        /// 发起请求，并获取数据体
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="millisecondsTimeout">等待超时时间</param>
        /// <param name="token">结束等待令箭</param>
        /// <returns></returns>
        Task<HttpResponseResult> RequestContentAsync(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default);
    }
}