using System.Threading;

namespace TouchSocket.Core;

/// <summary>
/// 定义一个等待数据的接口。
/// </summary>
/// <typeparam name="T">等待结果的类型。</typeparam>
public interface IWaitDataBase<T> : IDisposableObject
{
    /// <summary>
    /// 获取等待对象的状态。
    /// </summary>
    WaitDataStatus Status { get; }

    /// <summary>
    /// 获取等待结果。
    /// </summary>
    T WaitResult { get; }

    /// <summary>
    /// 取消等待。
    /// </summary>
    void Cancel();

    /// <summary>
    /// 重置等待对象。
    /// 设置<see cref="WaitResult"/>为null。然后重置状态为<see cref="WaitDataStatus.Default"/>。
    /// </summary>
    void Reset();

    /// <summary>
    /// 使等待的线程继续执行。
    /// </summary>
    /// <returns>如果操作成功，则返回true；否则返回false。</returns>
    bool Set();

    /// <summary>
    /// 使等待的线程继续执行，并设置等待结果。
    /// </summary>
    /// <param name="waitResult">等待结果。</param>
    /// <returns>如果操作成功，则返回true；否则返回false。</returns>
    bool Set(T waitResult);

    /// <summary>
    /// 设置取消令牌。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    void SetCancellationToken(CancellationToken cancellationToken);

    /// <summary>
    /// 设置等待结果。
    /// </summary>
    /// <param name="result">等待结果。</param>
    void SetResult(T result);
}
