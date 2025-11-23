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

namespace TouchSocket.Dmtp.FileTransfer;

/// <summary>
/// 可取消文件传输操作器
/// </summary>
public class CancellationFileOperator : FileOperator
{
    private readonly CancellationTokenSource m_tokenSource = new CancellationTokenSource();

    /// <inheritdoc/>
    public override CancellationToken Token
    {
        get
        {
            return this.m_tokenSource.Token;
        }

        set => throw new NotSupportedException();
    }

    /// <summary>
    /// 取消传输
    /// </summary>
    public void Cancel()
    {
        this.m_tokenSource.Cancel();
    }

    /// <summary>
    /// 在指定的时间之后取消传输。
    /// </summary>
    /// <param name="delay">延迟时间，在该时间之后取消传输</param>
    public void CancelAfter(TimeSpan delay)
    {
        // 使用内部的CancellationTokenSource来实现取消操作
        this.m_tokenSource.CancelAfter(delay);
    }
}