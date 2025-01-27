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

namespace TouchSocket.Core;


/// <summary>
/// 表示一个块段，用于异步操作中作为值任务的源，提供 <see cref="IBlockResult{T}"/> 类型的结果。
/// </summary>
/// <typeparam name="T">块段中元素的类型。</typeparam>
public abstract class BlockSegment<T> : ValueTaskSource<IBlockResult<T>>
{
    #region 字段

    private readonly AsyncAutoResetEvent m_resetEventForCompleteRead = new AsyncAutoResetEvent(false);
    private readonly BlockResult m_result;

    #endregion 字段

    /// <summary>
    /// 获取当前块段的结果。
    /// </summary>
    public IBlockResult<T> Result => this.m_result;

    /// <summary>
    /// 初始化BlockSegment类的新实例。
    /// </summary>
    public BlockSegment()
    {
        // 初始化块结果，与CompleteRead方法关联
        this.m_result = new BlockResult(this.CompleteRead);
    }

    /// <summary>
    /// 重置块段的状态，为下一次使用做准备。
    /// </summary>
    protected override void Reset()
    {
        // 重置等待读取完成的事件
        this.m_resetEventForCompleteRead.Reset();
        // 将块结果标记为未完成
        this.m_result.IsCompleted = false;
        // 清除结果中的内存数据
        this.m_result.Memory = default;
        // 清除结果中的消息
        this.m_result.Message = default;
        base.Reset();
    }

    /// <summary>
    /// 调度执行指定操作。
    /// </summary>
    /// <param name="action">要执行的操作。</param>
    /// <param name="state">操作的状态信息。</param>
    protected override void Scheduler(Action<object> action, object state)
    {
        // 定义一个局部函数来运行传递的操作
        void Run(object o)
        {
            action.Invoke(o);
        }
        // 不安全地将工作项加入线程池队列
        ThreadPool.UnsafeQueueUserWorkItem(Run, state);
    }

    /// <summary>
    /// 获取当前块段的结果。
    /// </summary>
    /// <returns>块段的结果。</returns>
    protected override IBlockResult<T> GetResult()
    {
        // 返回内部结果对象
        return this.m_result;
    }

    /// <summary>
    /// 异步地输入数据到块段中。
    /// </summary>
    /// <param name="memory">要输入的数据内存。</param>
    protected async Task InputAsync(ReadOnlyMemory<T> memory)
    {
        // 设置结果中的内存数据
        this.m_result.Memory = memory;
        // 标记为未完成
        this.Complete(false);
        // 等待读取完成
        await this.m_resetEventForCompleteRead.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 标记块段为完成，并可选地提供完成消息。
    /// </summary>
    /// <param name="msg">完成消息。</param>
    protected async Task Complete(string msg)
    {
        try
        {
            // 将结果标记为已完成
            this.m_result.IsCompleted = true;
            // 设置完成消息
            this.m_result.Message = msg;
            // 触发输入操作，以应用完成状态
            await this.InputAsync(default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
            // 异常情况下，不做处理
        }
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (disposing)
        {
            this.m_resetEventForCompleteRead.Set();
            this.m_resetEventForCompleteRead.SafeDispose();
        }
        base.Dispose(disposing);
    }

    private void CompleteRead()
    {
        this.m_resetEventForCompleteRead.Set();
    }

    #region Class

    internal class BlockResult : IBlockResult<T>
    {
        private readonly Action m_disAction;

        /// <summary>
        /// ReceiverResult
        /// </summary>
        /// <param name="disAction"></param>
        public BlockResult(Action disAction)
        {
            this.m_disAction = disAction;
        }

        public ReadOnlyMemory<T> Memory { get; set; }
        public bool IsCompleted { get; set; }
        public string Message { get; set; }

        public void Dispose()
        {
            this.m_disAction.Invoke();
        }
    }

    #endregion Class
}