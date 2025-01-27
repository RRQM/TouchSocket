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

namespace TouchSocket.Sockets;


/// <summary>
/// 定义了一个插件接口IServerStartedPlugin，该接口继承自IPlugin。
/// 该接口的目的是为那些需要在服务器启动时执行特定操作的插件提供一个标识。
/// </summary>
[DynamicMethod]
public interface IServerStartedPlugin : IPlugin
{
    /// <summary>
    /// 当服务器执行<see cref="IServiceBase.StartAsync"/>方法后时。
    /// <para>
    /// 注意：此处的事件并不意味着服务器已经成功启动，具体的启动状态请参考<see cref="ServiceStateEventArgs.ServerState"/>.
    /// </para>
    /// </summary>
    /// <param name="sender">触发事件的服务对象。</param>
    /// <param name="e">包含服务器状态信息的事件参数对象。</param>
    /// <returns>一个Task对象，标识异步操作的完成。</returns>
    Task OnServerStarted(IServiceBase sender, ServiceStateEventArgs e);

}