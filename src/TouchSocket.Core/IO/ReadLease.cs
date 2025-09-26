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

namespace TouchSocket.Core;

/// <summary>
/// 表示读取操作的租约结构体。
/// </summary>
/// <typeparam name="T">租约中包含的值的类型。</typeparam>
/// <remarks>
/// ReadLease是一个只读结构体，用于管理读取操作的生命周期和资源释放。
/// 当调用<see cref="Dispose"/>方法时表示消费完成，会触发相应的清理操作。
/// 此结构体实现了<see cref="IDisposable"/>接口以支持资源管理模式。
/// </remarks>
public readonly struct ReadLease<T> : IDisposable
{
    private readonly Action m_owner;

    internal ReadLease(Action disposeAction, T value, bool isCompleted)
    {
        this.m_owner = disposeAction;
        this.Value = value;
        this.IsCompleted = isCompleted;
    }

    /// <summary>
    /// 获取一个值，指示读取操作是否已完成。
    /// </summary>
    /// <value>如果读取操作已完成，则为<see langword="true"/>；否则为<see langword="false"/>。</value>
    /// <remarks>
    /// 当此属性为<see langword="true"/>时，表示读取操作已成功完成，无需额外的清理操作。
    /// 当此属性为<see langword="false"/>时，调用<see cref="Dispose"/>方法会触发清理操作。
    /// </remarks>
    public bool IsCompleted { get; }

    /// <summary>
    /// 获取租约中包含的值。
    /// </summary>
    /// <value>租约中包含的类型为<typeparamref name="T"/>的值。</value>
    /// <remarks>
    /// 此属性包含读取操作的结果值。该值在租约的生命周期内保持有效。
    /// </remarks>
    public T Value { get; }

    /// <summary>
    /// 释放租约相关的资源。
    /// </summary>
    /// <remarks>
    /// 此方法表示消费完成。如果读取操作未完成且存在释放操作，
    /// 则会调用构造时指定的释放回调来执行清理工作。
    /// 如果<see cref="IsCompleted"/>为<see langword="true"/>，则不会执行任何操作。
    /// </remarks>
    public void Dispose()
    {
        if (!this.IsCompleted && this.m_owner != null)
        {
            this.m_owner.Invoke();
        }
    }
}
