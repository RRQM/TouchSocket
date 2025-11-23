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

namespace TouchSocket.Core;

/// <summary>
/// 单线程状况的流式数据处理适配器。
/// </summary>
public abstract class SingleStreamDataHandlingAdapter : DataHandlingAdapter
{
    private long m_cacheSize;

    private volatile bool m_needReset = false;

    /// <summary>
    /// 缓存超时时间。默认1秒。
    /// </summary>
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 是否启用缓存超时。默认<see langword="false"/>。
    /// </summary>
    public bool CacheTimeoutEnable { get; set; } = false;

    /// <inheritdoc/>
    public override bool CanSendRequestInfo => false;

    /// <summary>
    /// 当接收数据处理完成后，回调该函数执行接收
    /// </summary>
    public Func<ReadOnlyMemory<byte>, IRequestInfo, Task> ReceivedAsyncCallBack { get; set; }

    /// <summary>
    /// 最后缓存的时间
    /// </summary>
    protected DateTimeOffset LastCacheTime { get; set; }

    /// <summary>
    /// 收到数据的切入点，该方法由框架自动调用。
    /// </summary>
    /// <param name="reader"></param>
    public async Task ReceivedInputAsync<TReader>(TReader reader)
        where TReader : class, IBytesReader
    {
        this.CacheVerify(ref reader);
        await this.PreviewReceivedAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_cacheSize = reader.BytesRemaining;
        this.LastCacheTime = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 校验并处理缓存数据的有效性。
    /// </summary>
    /// <typeparam name="TReader">实现了 <see cref="IBytesReader"/> 接口的类型。</typeparam>
    /// <param name="reader">字节读取器的引用。</param>
    protected void CacheVerify<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        if (this.m_cacheSize > 0)
        {
            if (this.CacheTimeoutEnable && this.LastCacheTime + this.CacheTimeout <= DateTimeOffset.UtcNow)
            {
                // 缓存已超时，跳过旧的缓存数据
                reader.Advance((int)this.m_cacheSize);
            }
            else if (this.m_needReset)
            {
                this.m_needReset = false;
                reader.Advance((int)this.m_cacheSize);
            }
        }
    }

    #region SendInput

    /// <summary>
    /// 发送输入数据到指定的写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IBytesWriter"/> 接口的写入器类型。</typeparam>
    /// <param name="writer">写入器的引用。</param>
    /// <param name="memory">要写入的数据内存块。</param>
    public virtual void SendInput<TWriter>(ref TWriter writer, in ReadOnlyMemory<byte> memory)
        where TWriter : IBytesWriter
    {
        writer.Write(memory.Span);
    }

    /// <summary>
    /// 发送输入数据到指定的写入器。
    /// 如果 <paramref name="requestInfo"/> 实现了 <see cref="IRequestInfoBuilder"/>，则调用其 Build 方法写入数据。
    /// 否则抛出异常。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IBytesWriter"/> 接口的写入器类型。</typeparam>
    /// <param name="writer">写入器的引用。</param>
    /// <param name="requestInfo">要写入的请求信息。</param>
    public virtual void SendInput<TWriter>(ref TWriter writer, IRequestInfo requestInfo)
        where TWriter : IBytesWriter
    {
        if (requestInfo is not IRequestInfoBuilder requestInfoBuilder)
        {
            throw new Exception();
        }
        requestInfoBuilder.Build(ref writer);
    }

    #endregion SendInput

    /// <summary>
    /// 处理已经经过预先处理后的数据
    /// </summary>
    /// <param name="memory">以二进制形式传递</param>
    /// <param name="requestInfo">以解析实例传递</param>
    protected virtual Task GoReceivedAsync(ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
    {
        return this.ReceivedAsyncCallBack == null ? EasyTask.CompletedTask : this.ReceivedAsyncCallBack.Invoke(memory, requestInfo);
    }

    /// <summary>
    /// 当接收到数据后预先处理数据,然后调用<see cref="GoReceivedAsync(ReadOnlyMemory{byte}, IRequestInfo)"/>处理数据
    /// </summary>
    /// <param name="reader"></param>
    protected abstract Task PreviewReceivedAsync<TReader>(TReader reader)
        where TReader : class, IBytesReader;


    /// <inheritdoc/>
    protected override void Reset()
    {
        this.m_needReset = true;
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
    }
}