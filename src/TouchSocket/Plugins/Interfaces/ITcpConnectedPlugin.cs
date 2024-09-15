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
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 定义了ITcpConnectedPlugin接口，它是通过TCP连接的插件应实现的接口。
    /// 这个接口扩展了IPlugin接口，增加了与TCP连接相关的功能和要求。
    /// </summary>
    public interface ITcpConnectedPlugin : IPlugin
    {
        /// <summary>
        /// 客户端连接成功后触发
        /// </summary>
        /// <param name="client">建立连接的客户端会话</param>
        /// <param name="e">连接事件参数</param>
        /// <returns>一个Task对象，标识异步操作</returns>
        Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e);
    }
}