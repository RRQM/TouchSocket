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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TouchSocket.Core;

public sealed class WaitHandlePool<T>
    where T : class, IWaitHandle
{
    private readonly int m_maxSign;
    private readonly int m_minSign;
    private readonly ConcurrentDictionary<int, AsyncWaitData<T>> m_waitDic = new();

    private int m_currentSign;


    public WaitHandlePool(int minSign = 1, int maxSign = int.MaxValue)
    {
        this.m_minSign = minSign;
        this.m_currentSign = minSign;
        this.m_maxSign = maxSign;
    }

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

    public AsyncWaitData<T> GetWaitDataAsync(T result, bool autoSign = true)
    {
        if (autoSign)
        {
            result.Sign = this.GetSign();
        }
        var waitDataAsyncSlim = new AsyncWaitData<T>(result.Sign, this.Remove, result);

        if (!this.m_waitDic.TryAdd(result.Sign, waitDataAsyncSlim))
        {
            ThrowHelper.ThrowInvalidOperationException($"The sign '{result.Sign}' is already in use.");
        }
        return waitDataAsyncSlim;
    }

    public AsyncWaitData<T> GetWaitDataAsync(out int sign)
    {
        sign = this.GetSign();
        var waitDataAsyncSlim = new AsyncWaitData<T>(sign, this.Remove, default);
        if (!this.m_waitDic.TryAdd(sign, waitDataAsyncSlim))
        {
            ThrowHelper.ThrowInvalidOperationException($"The sign '{sign}' is already in use.");
        }
        return waitDataAsyncSlim;
    }

    public bool Set(T result)
    {
        if (this.m_waitDic.TryRemove(result.Sign, out var waitDataAsync))
        {
            waitDataAsync.Set(result);
            return true;
        }
        return false;
    }

    public bool TryGetDataAsync(int sign, out AsyncWaitData<T> waitDataAsync)
    {
        return this.m_waitDic.TryGetValue(sign, out waitDataAsync);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSign()
    {
        var sign = Interlocked.Increment(ref this.m_currentSign);
        if (sign >= this.m_maxSign)
        {
            // 使用CAS操作来安全地重置计数器，避免竞态条件
            Interlocked.CompareExchange(ref this.m_currentSign, this.m_minSign, sign);
        }
        return sign;
    }

    private void Remove(int sign)
    {
        this.m_waitDic.TryRemove(sign, out _);
    }
}
