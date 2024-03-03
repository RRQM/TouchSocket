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

using System.Threading.Tasks;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http服务器终端接口
    /// </summary>
    public interface IHttpSocketClient : IHttpClientBase, ISocketClient
    {
        /// <summary>
        /// 当该连接是WebSocket时，可获取该对象，否则为null。
        /// </summary>
        IWebSocket WebSocket { get; }

        /// <summary>
        /// 转化Protocol协议标识为<see cref="Protocol.WebSocket"/>
        /// </summary>
        /// <param name="httpContext">Http上下文</param>
        Task<bool> SwitchProtocolToWebSocket(HttpContext httpContext);
    }
}