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
    /// 定义了一个ITcpClosedPlugin接口，该接口继承自IPlugin接口。
    /// 这个接口的目的是为插件提供一种标识，表明该插件支持处理TCP连接关闭时的操作。
    /// </summary>
    [DynamicMethod]
    public interface ITcpClosedPlugin : IPlugin
    {
        /// <summary>
        /// 在会话断开后触发。
        /// </summary>
        /// <param name="client">断开连接的TCP会话客户端。</param>
        /// <param name="e">会话断开事件的参数。</param>
        /// <returns>一个等待完成的任务。</returns>
        Task OnTcpClosed(ITcpSession client, ClosedEventArgs e);
    }
}