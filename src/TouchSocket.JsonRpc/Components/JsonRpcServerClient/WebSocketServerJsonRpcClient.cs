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

using System;
using System.Threading.Tasks;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;

namespace TouchSocket.JsonRpc
{
    internal sealed class WebSocketServerJsonRpcClient : JsonRpcActionClientBase
    {
        private readonly IHttpSocketClient m_client;

        public WebSocketServerJsonRpcClient(IHttpSocketClient client)
        {
            if (client.Protocol != Sockets.Protocol.WebSocket)
            {
                throw new Exception("必须完成WebSocket连接");
            }
            this.m_client = client;
        }

        protected override void SendJsonString(string jsonString)
        {
            this.m_client.SendWithWS(jsonString);
        }

        protected override Task SendJsonStringAsync(string jsonString)
        {
            return this.m_client.SendWithWSAsync(jsonString);
        }
    }
}