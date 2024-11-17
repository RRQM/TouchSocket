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
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcClientExtension
    /// </summary>
    public static class JsonRpcClientExtension
    {
        /// <summary>
        /// 标识是否为JsonRpc
        /// </summary>
        public static readonly DependencyProperty<bool> IsJsonRpcProperty =
            new("IsJsonRpc", false);

        /// <summary>
        /// IJsonRpcActionClient
        /// </summary>
        public static readonly DependencyProperty<IJsonRpcActionClient> JsonRpcActionClientProperty =
            new("JsonRpcActionClient", default);

        /// <summary>
        /// 获取<see cref="IsJsonRpcProperty"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        internal static bool GetIsJsonRpc(this IDependencyObject client)
        {
            return client.GetValue(IsJsonRpcProperty);
        }

        /// <summary>
        /// 设置<see cref="IsJsonRpcProperty"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="value"></param>
        internal static void SetIsJsonRpc(this IDependencyObject client, bool value = true)
        {
            client.SetValue(IsJsonRpcProperty, value);
        }

        /// <summary>
        /// 获取基于Tcp协议或者WebSocket协议的双工JsonRpc端
        /// </summary>
        /// <param name="sessionClient"></param>
        /// <returns></returns>
        public static IJsonRpcActionClient GetJsonRpcActionClient(this ISessionClient sessionClient)
        {
            if (sessionClient.TryGetValue(JsonRpcActionClientProperty, out var actionClient))
            {
                return actionClient;
            }

            if (sessionClient.Protocol == Protocol.Tcp)
            {
                actionClient = new TcpServerJsonRpcClient((ITcpSessionClient)sessionClient);
                sessionClient.SetValue(JsonRpcActionClientProperty, actionClient);
                return actionClient;
            }
            else if (sessionClient.Protocol == Protocol.WebSocket)
            {
                actionClient = new WebSocketServerJsonRpcClient((IHttpSessionClient)sessionClient);
                sessionClient.SetValue(JsonRpcActionClientProperty, actionClient);
                return actionClient;
            }
            else
            {
                throw new System.Exception("SessionClient必须是Tcp协议，或者完成WebSocket连接");
            }
        }
    }
}