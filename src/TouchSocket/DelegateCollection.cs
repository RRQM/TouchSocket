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

namespace TouchSocket.Sockets;

/// <summary>
/// 客户端已断开连接
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
/// <param name="client">断开连接的客户端</param>
/// <param name="e">断开连接事件参数</param>
/// <returns>任务</returns>
public delegate Task ClosedEventHandler<TClient>(TClient client, ClosedEventArgs e);

/// <summary>
/// 客户端即将断开连接
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
/// <param name="client">即将断开连接的客户端</param>
/// <param name="e">断开连接事件参数</param>
public delegate Task ClosingEventHandler<TClient>(TClient client, ClosingEventArgs e);

/// <summary>
/// 客户端已连接
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
/// <param name="client">已连接的客户端</param>
/// <param name="e">连接事件参数</param>
public delegate Task ConnectedEventHandler<TClient>(TClient client, ConnectedEventArgs e);

/// <summary>
/// 客户端正在连接
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
/// <param name="client">正在连接的客户端</param>
/// <param name="e">连接事件参数</param>
public delegate Task ConnectingEventHandler<TClient>(TClient client, ConnectingEventArgs e);

/// <summary>
/// 接收数据事件
/// </summary>
/// <param name="client">接收数据的客户端</param>
/// <param name="e">接收数据事件参数</param>
public delegate Task ReceivedEventHandler<TClient>(TClient client, ReceivedDataEventArgs e);

/// <summary>
/// 尝试获取客户端实例
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
/// <param name="id">客户端标识</param>
/// <param name="client">客户端实例</param>
/// <returns>是否成功获取客户端实例</returns>
public delegate bool TryOutEventHandler<TClient>(string id, out TClient client);

/// <summary>
/// UDP数据接收事件
/// </summary>
/// <typeparam name="TClient">客户端类型</typeparam>
/// <param name="client">接收数据的客户端</param>
/// <param name="e">UDP接收数据事件参数</param>
public delegate Task UdpReceivedEventHandler<TClient>(TClient client, UdpReceivedDataEventArgs e);