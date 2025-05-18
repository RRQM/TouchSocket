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
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了<see cref="INatSessionClient"/>接口。
/// 该接口专门用于处理需要网络地址转换（Nat）支持的TCP会话客户端操作。
/// </summary>
public interface INatSessionClient : ITcpSession, ITcpListenableClient, IClient, IIdClient
{
    /// <summary>
    /// 异步添加目标客户端。
    /// </summary>
    /// <param name="setupAction">配置操作委托，用于设置TouchSocket配置。</param>
    /// <returns>Task异步任务对象。</returns>
    Task AddTargetClientAsync(Action<TouchSocketConfig> setupAction);

    /// <summary>
    /// 异步添加目标客户端。
    /// </summary>
    /// <param name="client">要添加的TCP客户端。</param>
    /// <returns>Task异步任务对象。</returns>
    Task AddTargetClientAsync(NatTargetClient client);

    /// <summary>
    /// 移除目标客户端。
    /// </summary>
    /// <param name="client">要移除的TCP客户端。</param>
    /// <returns>如果移除成功则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    bool RemoveTargetClient(NatTargetClient client);

    /// <summary>
    /// 获取所有目标客户端。
    /// </summary>
    /// <returns>返回一个包含所有目标客户端的数组。</returns>
    IEnumerable<NatTargetClient> GetTargetClients();
}