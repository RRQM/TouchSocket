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

using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;


/// <summary>
/// 定义了接收操作结果的接收者接口。
/// 此接口继承自 IDisposableObject，表明接收者对象应该支持释放操作。
/// </summary>
/// <typeparam name="TResult">接收结果的类型，必须继承自<see cref="IReceiverResult"/>。</typeparam>
public interface IReceiver<TResult> : IDisposableObject where TResult : IReceiverResult
{
    /// <summary>
    /// 获取或设置是否启用缓存模式。
    /// <para>
    /// 设为<see langword="true"/>即启用缓存模式。在缓存模式下，<see cref="IReceiverResult.ByteBlock"/>的数据如果<see cref="ByteBlock.CanReadLength"/>大于0。
    /// 即会缓存未消费的数据。
    /// </para>
    /// </summary>
    bool CacheMode { get; set; }

    /// <summary>
    /// 获取或设置最大缓存大小。
    /// 这决定了缓存能够存储的最大数据量，以字节为单位。
    /// </summary>
    int MaxCacheSize { get; set; }


    /// <summary>
    /// 异步读取操作。
    /// </summary>
    /// <param name="token">用于取消异步读取操作的取消令牌。</param>
    /// <returns>一个<see cref="ValueTask{TResult}"/>，其结果是异步读取的数据。</returns>
    ValueTask<TResult> ReadAsync(CancellationToken token);
}