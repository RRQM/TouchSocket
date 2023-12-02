//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
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
            DependencyProperty<bool>.Register("IsJsonRpc", false);

        /// <summary>
        /// IJsonRpcActionClient
        /// </summary>
        public static readonly DependencyProperty<IJsonRpcActionClient> JsonRpcActionClientProperty =
            DependencyProperty<IJsonRpcActionClient>.Register("JsonRpcActionClient", default);

        /// <summary>
        /// 获取<see cref="IsJsonRpcProperty"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool GetIsJsonRpc(this ITcpClientBase client)
        {
            return client.GetValue(IsJsonRpcProperty);
        }

        /// <summary>
        /// 设置<see cref="IsJsonRpcProperty"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="value"></param>
        public static void SetIsJsonRpc(this ITcpClientBase client, bool value = true)
        {
            client.SetValue(IsJsonRpcProperty, value);
        }

        /// <summary>
        /// 获取基于Tcp协议或者WebSocket协议的双工JsonRpc端
        /// </summary>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        public static IJsonRpcActionClient GetJsonRpcActionClient(this ISocketClient socketClient)
        {
            if (socketClient.TryGetValue(JsonRpcActionClientProperty, out var actionClient))
            {
                return actionClient;
            }

            if (socketClient.Protocol == Sockets.Protocol.Tcp)
            {
                actionClient = new TcpServerJsonRpcClient(socketClient);
                socketClient.SetValue(JsonRpcActionClientProperty, actionClient);
                return actionClient;
            }
            else if (socketClient.Protocol == Sockets.Protocol.WebSocket)
            {
                actionClient = new WebSocketServerJsonRpcClient((IHttpSocketClient)socketClient);
                socketClient.SetValue(JsonRpcActionClientProperty, actionClient);
                return actionClient;
            }
            else
            {
                throw new System.Exception("SocketClient必须是Tcp协议，或者完成WebSocket连接");
            }
        }
    }
}