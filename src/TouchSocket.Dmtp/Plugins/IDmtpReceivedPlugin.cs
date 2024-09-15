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

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 定义了一个插件接口，该插件用于处理接收到的Dmtp消息。
    /// </summary>
    public interface IDmtpReceivedPlugin : IPlugin
    {
        /// <summary>
        /// 当接收到DmtpMessage数据时触发。
        /// 此方法允许插件处理通过Dmtp协议收到的消息。
        /// </summary>
        /// <param name="client">发送消息的客户端对象，实现了IDmtpActorObject接口。</param>
        /// <param name="e">包含收到消息的详细信息的事件参数。</param>
        /// <returns>一个Task对象，表明该方法是一个异步操作。</returns>
        Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e);
    }
}