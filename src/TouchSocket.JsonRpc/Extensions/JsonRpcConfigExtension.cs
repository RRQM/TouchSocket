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

using TouchSocket.Core;
using TouchSocket.JsonRpc;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// JsonRpcConfigExtension
    /// </summary>
    public static class JsonRpcConfigExtension
    {
        /// <summary>
        /// 构建WebSocketJsonRpc类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithWebSocketJsonRpcClient<TClient>(this TouchSocketConfig config) where TClient : IWebSocketJsonRpcClient, new()
        {
            return config.BuildClient<TClient>();
        }

        /// <summary>
        /// 构建WebSocketJsonRpc类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static WebSocketJsonRpcClient BuildWithWebSocketJsonRpcClient(this TouchSocketConfig config)
        {
            return BuildWithWebSocketJsonRpcClient<WebSocketJsonRpcClient>(config);
        }

        /// <summary>
        /// 构建HttpJsonRpc类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithHttpJsonRpcClient<TClient>(this TouchSocketConfig config) where TClient : IHttpJsonRpcClient, new()
        {
            return config.BuildClient<TClient>();
        }

        /// <summary>
        /// 构建HttpJsonRpc类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpJsonRpcClient BuildWithHttpJsonRpcClient(this TouchSocketConfig config)
        {
            return BuildWithHttpJsonRpcClient<HttpJsonRpcClient>(config);
        }

        /// <summary>
        /// 构建TcpJsonRpc类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithTcpJsonRpcClient<TClient>(this TouchSocketConfig config) where TClient : ITcpJsonRpcClient, new()
        {
            return config.BuildClient<TClient>();
        }

        /// <summary>
        /// 构建TcpJsonRpc类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpJsonRpcClient BuildWithTcpJsonRpcClient(this TouchSocketConfig config)
        {
            return BuildWithTcpJsonRpcClient<TcpJsonRpcClient>(config);
        }
    }
}