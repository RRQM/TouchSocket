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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 具有Udp终结点的发送
/// </summary>
public interface IUdpClientSender : ISender, IUdpRequestInfoSender
{
    /// <summary>
    /// 异步组合发送数据。
    /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
    /// </summary>
    /// <param name="endPoint">远程终结点</param>
    /// <param name="memory">只读内存块，包含待发送的数据</param>
    /// <param name="token">可取消令箭</param>
    /// <exception cref="OverlengthException">发送数据超长</exception>
    /// <exception cref="Exception">其他异常</exception>
    /// <returns>一个表示异步操作的Task对象</returns>
    Task SendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory,CancellationToken token = default);
}