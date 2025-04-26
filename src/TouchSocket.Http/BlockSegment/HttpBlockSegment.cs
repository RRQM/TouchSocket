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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;
class HttpBlockSegment : BlockSegment<IReadOnlyMemoryBlockResult>
{
    HttpReadOnlyMemoryBlockResult m_blockResult;
    protected override IReadOnlyMemoryBlockResult CreateResult(Action actionForDispose)
    {
        m_blockResult = new HttpReadOnlyMemoryBlockResult(actionForDispose);
        return m_blockResult;
    }

    internal async Task InternalComplete(string msg)
    {
        try
        {
            this.m_blockResult.IsCompleted = true;
            this.m_blockResult.Message = msg;
            await this.TriggerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
    }

    internal Task InternalInputAsync(in ReadOnlyMemory<byte> memory)
    {
        this.m_blockResult.Memory = memory;
        return base.TriggerAsync();
    }

    protected override void CompleteRead()
    {
        // 清除结果中的内存数据
        this.m_blockResult.Memory = default;
        // 清除结果中的消息
        this.m_blockResult.Message = default;
        base.CompleteRead();
    }


    internal void InternalReset()
    {
        // 将块结果标记为未完成
        this.m_blockResult.IsCompleted = false;
        // 清除结果中的内存数据
        this.m_blockResult.Memory = default;
        // 清除结果中的消息
        this.m_blockResult.Message = default;
    }

    internal ValueTask<IReadOnlyMemoryBlockResult> InternalValueWaitAsync(CancellationToken cancellationToken)
    {
        return base.ProtectedReadAsync(cancellationToken);
    }
}

