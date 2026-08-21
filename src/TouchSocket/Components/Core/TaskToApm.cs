using System.IO;
using System.Diagnostics.Contracts;

namespace System.Threading.Tasks;

/// <summary>
/// 参考Mono的Ssl实现
/// https://github.com/mono/mono/pull/19476/changes
/// </summary>
internal static class TaskToApm
{
    public static IAsyncResult Begin(Task task, AsyncCallback callback, object state)
    {
        Contract.Requires(task != null);

        IAsyncResult asyncResult;
        if (task.IsCompleted)
        {
            asyncResult = new TaskWrapperAsyncResult(task, state, completedSynchronously: true);
            if (callback != null)
                callback(asyncResult);
        }
        else
        {
            asyncResult = task.AsyncState == state ? task : new TaskWrapperAsyncResult(task, state, completedSynchronously: false);
            if (callback != null)
                InvokeCallbackWhenTaskCompletes(task, callback, asyncResult);
        }
        return asyncResult;
    }

    public static void End(IAsyncResult asyncResult)
    {
        Task task;

        var twar = asyncResult as TaskWrapperAsyncResult;
        if (twar != null)
        {
            task = twar.Task;
            Contract.Assert(task != null, "TaskWrapperAsyncResult should never wrap a null Task.");
        }
        else
        {
            task = asyncResult as Task;
        }

        if (task == null)
            throw new ArgumentNullException(nameof(task), "WrongAsyncResult");
        task.GetAwaiter().GetResult();
    }

    public static TResult End<TResult>(IAsyncResult asyncResult)
    {
        Task<TResult> task;

        var twar = asyncResult as TaskWrapperAsyncResult;
        if (twar != null)
        {
            task = twar.Task as Task<TResult>;
            Contract.Assert(twar.Task != null, "TaskWrapperAsyncResult should never wrap a null Task.");
        }
        else
        {
            task = asyncResult as Task<TResult>;
        }

        if (task == null)
            throw new ArgumentNullException(nameof(task), "WrongAsyncResult");
        return task.GetAwaiter().GetResult();
    }

    private static void InvokeCallbackWhenTaskCompletes(Task antecedent, AsyncCallback callback, IAsyncResult asyncResult)
    {
        Contract.Requires(antecedent != null);
        Contract.Requires(callback != null);
        Contract.Requires(asyncResult != null);

        antecedent.ConfigureAwait(continueOnCapturedContext: false)
                  .GetAwaiter()
                  .OnCompleted(() => callback(asyncResult));

    }

    private sealed class TaskWrapperAsyncResult : IAsyncResult
    {
        internal readonly Task Task;
        private readonly object m_state;
        private readonly bool m_completedSynchronously;

        internal TaskWrapperAsyncResult(Task task, object state, bool completedSynchronously)
        {
            Contract.Requires(task != null);
            Contract.Requires(!completedSynchronously || task.IsCompleted, "If completedSynchronously is true, the task must be completed.");

            this.Task = task;
            m_state = state;
            m_completedSynchronously = completedSynchronously;
        }

        object IAsyncResult.AsyncState { get { return m_state; } }
        bool IAsyncResult.CompletedSynchronously { get { return m_completedSynchronously; } }
        bool IAsyncResult.IsCompleted { get { return this.Task.IsCompleted; } }
        WaitHandle IAsyncResult.AsyncWaitHandle { get { return ((IAsyncResult)this.Task).AsyncWaitHandle; } }
    }
}
