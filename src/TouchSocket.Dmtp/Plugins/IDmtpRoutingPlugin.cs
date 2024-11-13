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
    /// 定义了IDmtpRoutingPlugin接口，它是IPlugin接口的子接口，用于处理DMTP路由插件的转发逻辑。
    /// </summary>
    [DynamicMethod]
    public interface IDmtpRoutingPlugin : IPlugin
    {
        /// <summary>
        /// 当需要转发路由包时调用的方法。
        /// 该方法详细描述了在路由包需要被转发时所采取的行动。
        /// </summary>
        /// <param name="client">一个IDmtpActorObject类型的参数，表示与当前会话相关的客户端信息。</param>
        /// <param name="e">一个PackageRouterEventArgs类型的参数，包含了需要被路由的包的相关信息。</param>
        /// <returns>返回一个Task对象，表明该方法是一个异步方法。</returns>
        Task OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e);
    }
}