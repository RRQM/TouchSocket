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

namespace TouchSocket.Dmtp;

/// <summary>
/// 由宿主插件持有并释放、可以在重连后继续使用的功能件。
/// </summary>
/// <remarks>连接结束时清理本次连接的资源并保留恢复配置；宿主插件负责在释放时调用 Dispose。</remarks>
public interface IReconnectableActor : IActor
{
    /// <summary>
    /// 当前连接结束时清理会话资源。
    /// </summary>
    void OnDisconnected();
}
