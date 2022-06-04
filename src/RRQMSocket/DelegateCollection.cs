//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;

using RRQMSocket;
using System.Net;

/// <summary>
/// 显示信息
/// </summary>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void RRQMMessageEventHandler<TClient>(TClient client, MesEventArgs e) where TClient : IClient;

/// <summary>
/// 普通通知
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void RRQMEventHandler<TClient>(TClient client, RRQMEventArgs e) where TClient : IClient;

/// <summary>
/// 客户端连接
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void RRQMTcpClientConnectingEventHandler<TClient>(TClient client, ClientConnectingEventArgs e) where TClient : ITcpClientBase;

/// <summary>
/// 客户端断开连接
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void RRQMTcpClientDisconnectedEventHandler<TClient>(TClient client, ClientDisconnectedEventArgs e) where TClient : ITcpClientBase;

/// <summary>
/// 正在连接事件
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void RRQMClientOperationEventHandler<TClient>(TClient client, ClientOperationEventArgs e) where TClient : IClient;


/// <summary>
/// 插件数据
/// </summary>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void RRQMPluginReceivedEventHandler<TClient>(TClient client, ReceivedDataEventArgs e) where TClient : IClient;

/// <summary>
/// 普通数据
/// </summary>
/// <param name="client"></param>
/// <param name="byteBlock"></param>
/// <param name="requestInfo"></param>
public delegate void RRQMReceivedEventHandler<TClient>(TClient client, ByteBlock byteBlock, IRequestInfo requestInfo) where TClient : IClient;


/// <summary>
/// UDP接收
/// </summary>
/// <param name="endpoint"></param>
/// <param name="byteBlock"></param>
/// <param name="requestInfo"></param>
public delegate void RRQMUDPByteBlockEventHandler(EndPoint endpoint, ByteBlock byteBlock, IRequestInfo requestInfo);