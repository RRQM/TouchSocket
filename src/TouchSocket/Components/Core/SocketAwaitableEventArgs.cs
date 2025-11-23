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

using System.Net.Sockets;
using System.Threading.Tasks.Sources;

namespace TouchSocket.Sockets;

internal abstract class SocketAwaitableEventArgs<TResult> : SocketAsyncEventArgs, IValueTaskSource<TResult>
    where TResult : struct
{

#if NET6_0_OR_GREATER
    public SocketAwaitableEventArgs()
       : base(true)
    {
       
    }
#else
    public SocketAwaitableEventArgs()
    {

    }
#endif



    protected ManualResetValueTaskSourceCore<TResult> m_core = new ManualResetValueTaskSourceCore<TResult>();

    public bool RunContinuationsAsynchronously { get => this.m_core.RunContinuationsAsynchronously; set => this.m_core.RunContinuationsAsynchronously = value; }

    TResult IValueTaskSource<TResult>.GetResult(short cancellationToken)
    {
        return this.m_core.GetResult(cancellationToken);
    }

    ValueTaskSourceStatus IValueTaskSource<TResult>.GetStatus(short cancellationToken)
    {
        return this.m_core.GetStatus(cancellationToken);
    }

    void IValueTaskSource<TResult>.OnCompleted(Action<object> continuation, object state, short cancellationToken, ValueTaskSourceOnCompletedFlags flags)
    {
        try
        {
            this.m_core.OnCompleted(continuation, state, cancellationToken, flags);
        }
        catch (Exception ex)
        {
            // 如果可能，尝试通知异常
            try
            {
                this.m_core.SetException(ex);
            }
            catch
            {
                // 忽略SetException的异常，避免递归异常
            }
        }
    }

    protected override void OnCompleted(SocketAsyncEventArgs e)
    {
        this.m_core.SetResult(this.GetResult());
        base.OnCompleted(e);
    }

    protected abstract TResult GetResult();
}