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

namespace TouchSocket.Rpc;


public class InvokeOption : IInvokeOption
{
    /// <summary>
    /// 默认设置。
    /// Timeout=5000ms
    /// </summary>
    public static InvokeOption OnlySend => new InvokeOption(millisecondsTimeout: 5000)
    {
        FeedbackType = FeedbackType.OnlySend
    };

    /// <summary>
    /// 默认设置。
    /// Timeout=5000ms
    /// </summary>
    public static InvokeOption WaitInvoke => new InvokeOption(millisecondsTimeout: 5000)
    {
        FeedbackType = FeedbackType.WaitInvoke
    };

    /// <summary>
    /// 默认设置。
    /// Timeout=5000 ms
    /// </summary>
    public static InvokeOption WaitSend => new InvokeOption(millisecondsTimeout: 5000)
    {
        FeedbackType = FeedbackType.WaitSend
    };


    private CancellationToken m_token;

    private readonly CancellationTokenSource m_tokenSource;


    public InvokeOption()
    {
    }


    public InvokeOption(int millisecondsTimeout)
    {
        this.m_tokenSource = new CancellationTokenSource(millisecondsTimeout);
    }


    public FeedbackType FeedbackType { get; init; } = FeedbackType.WaitInvoke;


    public CancellationToken Token
    {
        get => this.m_tokenSource?.Token ?? this.m_token;
        init => this.m_token = value;
    }
}