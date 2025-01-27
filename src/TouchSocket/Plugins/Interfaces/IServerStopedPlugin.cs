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

using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;


/// <summary>
/// 定义了一个接口，用于标识和处理服务器停止时的插件行为。
/// </summary>
[DynamicMethod]
public interface IServerStopedPlugin : IPlugin
{
    /// <summary>
    /// 当服务器调用<see cref="IServiceBase.StopAsync"/>或者<see cref="IDisposable.Dispose"/>时
    /// </summary>
    /// <param name="sender">发送停止或释放操作的服务对象</param>
    /// <param name="e">包含停止或释放操作相关信息的事件参数</param>
    /// <returns>一个Task对象，表示异步操作的完成</returns>
    Task OnServerStoped(IServiceBase sender, ServiceStateEventArgs e);
}