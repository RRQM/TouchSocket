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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 提供一个基于Dmtp协议的，可以独立读写的通道。
/// </summary>
public partial interface IDmtpChannel : ISender, IDisposableObject
{
    /// <summary>
    /// 获取一个值，该值指示通道是否可以继续读取数据。
    /// <para>当通道状态为<see cref="ChannelStatus.Default"/>或<see cref="ChannelStatus.HoldOn"/>时返回<see langword="true"/>。</para>
    /// </summary>
    bool CanRead { get; }

    /// <summary>
    /// 获取一个值，该值指示通道是否可以继续写入数据。
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    /// 通道Id
    /// </summary>
    int Id { get; }

    /// <summary>
    /// 最后一次操作时显示消息
    /// </summary>
    string LastOperationMes { get; }

    /// <summary>
    /// 获取上次操作的时间。
    /// </summary>
    DateTimeOffset LastOperationTime { get; }

    /// <summary>
    /// 元数据
    /// </summary>
    Metadata Metadata { get; }

    /// <summary>
    /// 状态
    /// </summary>
    ChannelStatus Status { get; }

    /// <summary>
    /// 目的Id地址。仅当该通道由两个客户端打通时有效。
    /// </summary>
    string TargetId { get; }

    /// <summary>
    /// 异步取消操作
    /// </summary>
    /// <param name="operationMes">可选参数，用于提供取消操作的详细信息</param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回一个Task对象，表示异步取消操作的完成</returns>
    Task<Result> CancelAsync(string operationMes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步完成操作
    /// </summary>
    /// <param name="operationMes">操作信息，可选参数，默认为<see langword="null"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回一个Task对象，表示异步操作的完成</returns>
    Task<Result> CompleteAsync(string operationMes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步调用继续
    /// <para>调用该指令时，接收方会获取到Msg，然后继续迭代。</para>
    /// </summary>
    /// <param name="operationMes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> HoldOnAsync(string operationMes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步读取数据
    /// </summary>
    /// <param name="token">取消令箭</param>
    /// <returns>返回读取到的数据。当通道状态发生变化或出现错误时会抛出相应异常。</returns>
    /// <exception cref="InvalidOperationException">通道状态不允许读取数据</exception>
    /// <exception cref="ObjectDisposedException">通道已被释放</exception>
    /// <exception cref="OperationCanceledException">操作被取消</exception>
    /// <exception cref="Exception">其他异常</exception>
    Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken token = default);
}