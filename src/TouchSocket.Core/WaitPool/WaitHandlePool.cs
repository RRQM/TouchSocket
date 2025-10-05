// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个等待句柄池，用于管理具有等待功能的对象集合。
/// </summary>
/// <typeparam name="T">等待句柄的类型，必须实现<see cref="IWaitHandle"/>接口并且是引用类型。</typeparam>
/// <remarks>
/// WaitHandlePool提供了对等待句柄的集中管理，支持自动签名生成、等待数据创建和池内对象的生命周期管理。
/// 使用线程安全的并发字典来存储等待数据，支持高并发场景下的操作。
/// 签名生成采用原子递增方式，确保在指定范围内的唯一性。
/// </remarks>
public sealed class WaitHandlePool<T>
    where T : class, IWaitHandle
{
    private readonly int m_maxSign;
    private readonly int m_minSign;
    private readonly Action<int> m_remove;
    private readonly ConcurrentDictionary<int, AsyncWaitData<T>> m_waitDic = new();
    private int m_currentSign;

    /// <summary>
    /// 初始化<see cref="WaitHandlePool{T}"/>类的新实例。
    /// </summary>
    /// <param name="minSign">签名的最小值，默认为1。</param>
    /// <param name="maxSign">签名的最大值，默认为<see cref="int.MaxValue"/>。</param>
    /// <remarks>
    /// 签名范围用于控制自动生成的唯一标识符的取值范围。
    /// 当签名达到最大值时，会自动重置到最小值重新开始分配。
    /// </remarks>
    public WaitHandlePool(int minSign = 1, int maxSign = int.MaxValue)
    {
        this.m_minSign = minSign;
        this.m_currentSign = minSign;
        this.m_maxSign = maxSign;
        this.m_remove = this.Remove;
    }

    /// <summary>
    /// 取消池中所有等待操作。
    /// </summary>
    /// <remarks>
    /// 此方法会遍历池中所有的等待数据，并调用其<see cref="AsyncWaitData{T}.Cancel"/>方法来取消等待。
    /// 取消后的等待数据会从池中移除。适用于应用程序关闭或需要批量取消所有等待操作的场景。
    /// </remarks>
    public void CancelAll()
    {
        var signs = this.m_waitDic.Keys.ToList();
        foreach (var sign in signs)
        {
            if (this.m_waitDic.TryRemove(sign, out var item))
            {
                item.Cancel();
            }
        }
    }

    /// <summary>
    /// 获取与指定结果关联的异步等待数据。
    /// </summary>
    /// <param name="result">要关联的结果对象。</param>
    /// <param name="autoSign">指示是否自动为结果对象分配签名，默认为<see langword="true"/>。</param>
    /// <returns>创建的<see cref="AsyncWaitData{T}"/>实例。</returns>
    /// <exception cref="InvalidOperationException">当指定的签名已被使用时抛出。</exception>
    /// <remarks>
    /// 如果<paramref name="autoSign"/>为<see langword="true"/>，方法会自动为结果对象生成唯一签名。
    /// 创建的等待数据会被添加到池中，直到被设置结果或取消时才会移除。
    /// </remarks>
    public AsyncWaitData<T> GetWaitDataAsync(T result, bool autoSign = true)
    {
        if (autoSign)
        {
            result.Sign = this.GetSign();
        }
        var waitDataAsyncSlim = new AsyncWaitData<T>(result.Sign, this.m_remove, result);

        if (!this.m_waitDic.TryAdd(result.Sign, waitDataAsyncSlim))
        {
            ThrowHelper.ThrowInvalidOperationException($"The sign '{result.Sign}' is already in use.");
        }
        return waitDataAsyncSlim;
    }

    /// <summary>
    /// 获取具有自动生成签名的异步等待数据。
    /// </summary>
    /// <param name="sign">输出参数，返回自动生成的签名值。</param>
    /// <returns>创建的<see cref="AsyncWaitData{T}"/>实例。</returns>
    /// <exception cref="InvalidOperationException">当生成的签名已被使用时抛出。</exception>
    /// <remarks>
    /// 此方法会自动生成唯一签名，并创建不包含挂起数据的等待对象。
    /// 适用于只需要等待通知而不关心具体数据内容的场景。
    /// </remarks>
    public AsyncWaitData<T> GetWaitDataAsync(out int sign)
    {
        sign = this.GetSign();
        var waitDataAsyncSlim = new AsyncWaitData<T>(sign, this.m_remove, default);
        if (!this.m_waitDic.TryAdd(sign, waitDataAsyncSlim))
        {
            ThrowHelper.ThrowInvalidOperationException($"The sign '{sign}' is already in use.");
        }
        return waitDataAsyncSlim;
    }

    /// <summary>
    /// 使用指定结果设置对应签名的等待操作。
    /// </summary>
    /// <param name="result">包含签名和结果数据的对象。</param>
    /// <returns>如果成功设置等待操作则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    /// <remarks>
    /// 此方法根据结果对象的签名查找对应的等待数据，并设置其结果。
    /// 设置成功后，等待数据会从池中移除，正在等待的任务会被完成。
    /// 如果找不到对应签名的等待数据，则返回<see langword="false"/>。
    /// </remarks>
    public bool Set(T result)
    {
        if (this.m_waitDic.TryRemove(result.Sign, out var waitDataAsync))
        {
            waitDataAsync.Set(result);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 尝试获取指定签名的异步等待数据。
    /// </summary>
    /// <param name="sign">要查找的签名。</param>
    /// <param name="waitDataAsync">输出参数，如果找到则返回对应的等待数据；否则为<see langword="null"/>。</param>
    /// <returns>如果找到指定签名的等待数据则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    /// <remarks>
    /// 此方法允许查询池中是否存在特定签名的等待数据，而不会修改池的状态。
    /// 适用于需要检查等待状态或获取等待数据进行进一步操作的场景。
    /// </remarks>
    public bool TryGetDataAsync(int sign, out AsyncWaitData<T> waitDataAsync)
    {
        return this.m_waitDic.TryGetValue(sign, out waitDataAsync);
    }

    /// <summary>
    /// 生成下一个可用的唯一签名。
    /// </summary>
    /// <returns>生成的唯一签名值。</returns>
    /// <remarks>
    /// 使用原子递增操作确保签名的唯一性和线程安全性。
    /// 当签名达到最大值时，会重新开始分配以避免溢出。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSign()
    {
        while (true)
        {
            var currentSign = this.m_currentSign;
            var nextSign = currentSign >= this.m_maxSign ? this.m_minSign : currentSign + 1;

            if (Interlocked.CompareExchange(ref this.m_currentSign, nextSign, currentSign) == currentSign)
            {
                return nextSign;
            }
            // 如果CAS失败，继续重试
        }
    }

    /// <summary>
    /// 从池中移除指定签名的等待数据。
    /// </summary>
    /// <param name="sign">要移除的签名。</param>
    /// <remarks>
    /// 此方法由等待数据在释放时自动调用，确保池中不会保留已完成或已取消的等待对象。
    /// </remarks>
    private void Remove(int sign)
    {
        this.m_waitDic.TryRemove(sign, out _);
    }
}