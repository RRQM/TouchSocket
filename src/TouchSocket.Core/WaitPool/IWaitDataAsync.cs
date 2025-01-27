using System;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 异步等待数据接口。
/// </summary>
/// <typeparam name="T">等待结果的类型。</typeparam>
public interface IWaitDataAsync<T> : IWaitDataBase<T>
{
    /// <summary>
    /// 异步等待指定的时间间隔。
    /// </summary>
    /// <param name="timeSpan">等待的时间间隔。</param>
    /// <returns>等待数据的状态。</returns>
    Task<WaitDataStatus> WaitAsync(TimeSpan timeSpan);

    /// <summary>
    /// 异步等待指定的毫秒数。
    /// </summary>
    /// <param name="millisecond">等待的毫秒数。</param>
    /// <returns>等待数据的状态。</returns>
    Task<WaitDataStatus> WaitAsync(int millisecond);
}
