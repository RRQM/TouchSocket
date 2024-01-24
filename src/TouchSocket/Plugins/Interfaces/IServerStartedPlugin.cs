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
    /// IServerStartedPlugin
    /// </summary>
    public interface IServerStartedPlugin<in TServer> : IPlugin where TServer : IService
    {
        /// <summary>
        /// 当服务器执行<see cref="IService.Start"/>后时。
        /// <para>
        /// 注意：此处并不表示服务器成功启动，具体状态请看<see cref="ServiceStateEventArgs.ServerState"/>
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnServerStarted(TServer sender, ServiceStateEventArgs e);
    }

    /// <summary>
    /// IServerStartedPlugin
    /// </summary>
    public interface IServerStartedPlugin : IServerStartedPlugin<IService>
    {
    }
}