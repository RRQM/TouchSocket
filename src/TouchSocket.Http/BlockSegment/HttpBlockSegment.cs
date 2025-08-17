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

namespace TouchSocket.Http;


//internal class HttpBlockSegment
//{
//    private readonly AsyncExchange<ReadOnlyMemory<byte>> m_asyncExchange = new();

//    private readonly HttpReadOnlyMemoryBlockResult m_blockResult = new HttpReadOnlyMemoryBlockResult();

//    public HttpBlockSegment()
//    {
//        this.m_asyncExchange.Complete();
//    }

//    internal void InternalComplete()
//    {
//        this.m_asyncExchange.Complete();
//    }

//    internal ValueTask InternalInputAsync(in ReadOnlyMemory<byte> memory, CancellationToken token)
//    {
//        return this.m_asyncExchange.WriteAsync(memory, token);
//    }

//    internal void InternalReset()
//    {
//        this.m_asyncExchange.Reset();
//    }

//    internal async ValueTask<IReadOnlyMemoryBlockResult> InternalValueWaitAsync(CancellationToken token)
//    {
//        var readLease = await this.m_asyncExchange.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//        var memory = readLease.Value;
//        m_blockResult.DisposeAction = readLease.Dispose;
//        m_blockResult.IsCompleted = readLease.IsCompleted;
//        m_blockResult.Memory = memory;

//        return m_blockResult;
//    }
//}

//class HttpBlockSegment : BlockSegment<IReadOnlyMemoryBlockResult>
//{
//    HttpReadOnlyMemoryBlockResult m_blockResult;
//    protected override IReadOnlyMemoryBlockResult CreateResult(Action actionForDispose)
//    {
//        this.m_blockResult = new HttpReadOnlyMemoryBlockResult() { DisposeAction= actionForDispose };
//        return this.m_blockResult;
//    }

//    internal async Task InternalComplete()
//    {
//        try
//        {
//            this.m_blockResult.IsCompleted = true;
//            this.m_blockResult.Message = string.Empty;
//            await this.TriggerAsync(CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//        }
//        catch
//        {
//        }
//    }

//    internal Task InternalInputAsync(in ReadOnlyMemory<byte> memory, CancellationToken token)
//    {
//        this.m_blockResult.Memory = memory;
//        return base.TriggerAsync(token);
//    }

//    protected override void CompleteRead()
//    {
//        this.m_blockResult.Memory = ReadOnlyMemory<byte>.Empty;
//        this.m_blockResult.Message = default;
//        base.CompleteRead();
//    }


//    internal void InternalReset()
//    {
//        // 将块结果标记为未完成
//        this.m_blockResult.IsCompleted = false;
//        this.m_blockResult.Memory = ReadOnlyMemory<byte>.Empty;
//        // 清除结果中的消息
//        this.m_blockResult.Message = default;
//    }

//    internal ValueTask<IReadOnlyMemoryBlockResult> InternalValueWaitAsync(CancellationToken cancellationToken)
//    {
//        return base.ProtectedReadAsync(cancellationToken);
//    }
//}


