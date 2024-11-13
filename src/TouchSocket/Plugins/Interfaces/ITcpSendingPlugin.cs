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
    /// 定义了一个ITcpSendingPlugin接口，该接口继承自IPlugin接口。
    /// 用于标识插件在系统中负责TCP发送功能。
    /// </summary>
    [DynamicMethod]
    public interface ITcpSendingPlugin : IPlugin
    {
        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// 此方法主要用于在数据发送前执行额外的操作，例如日志记录或数据修改。
        /// </summary>
        /// <param name="client">表示与客户端的TCP会话。</param>
        /// <param name="e">包含发送数据的事件参数。</param>
        /// <returns>一个Task对象，表示异步操作的结果。</returns>
        Task OnTcpSending(ITcpSession client, SendingEventArgs e);
    }
}