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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 单线程状况的流式数据处理适配器。
/// </summary>
public abstract class SingleStreamDataHandlingAdapter : DataHandlingAdapter
{
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
    public Func<IByteBlockReader, IRequestInfo, Task> ReceivedAsyncCallBack { get; set; }

    /// <summary>
    /// 当发送数据处理完成后，回调该函数执行异步发送
    /// </summary>
    public Func<ReadOnlyMemory<byte>, CancellationToken, Task> SendAsyncCallBack { get; set; }

    /// <summary>
    /// 是否在收到数据时，即刷新缓存时间。默认<see langword="true"/>。
    /// <list type="number">
    /// <item>当设为<see langword="true"/>时，将弱化<see cref="CacheTimeout"/>的作用，只要一直有数据，则缓存不会过期。</item>
    /// <item>当设为<see langword="false"/>时，则在<see cref="CacheTimeout"/>的时效内。必须完成单个缓存的数据。</item>
    /// </list>
    /// </summary>
    public bool UpdateCacheTimeWhenRev { get; set; } = true;

    /// <summary>
    /// 最后缓存的时间
    /// </summary>
    protected DateTimeOffset LastCacheTime { get; set; }

    /// <summary>
    /// 收到数据的切入点，该方法由框架自动调用。
    /// </summary>
    /// <param name="reader"></param>
    public async Task ReceivedInputAsync(IByteBlockReader reader)
    {
        try
        {
            await this.PreviewReceivedAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch (Exception ex)
        {
            this.OnError(ex, ex.Message, true, true);
        }
    }

    #region SendInput

    /// <summary>
    /// 发送数据的切入点，该方法由框架自动调用。
    /// </summary>
    /// <param name="requestInfo"></param>
    /// <param name="token">可取消令箭</param>
    /// <returns></returns>
    public Task SendInputAsync(IRequestInfo requestInfo, CancellationToken token)
    {
        return this.PreviewSendAsync(requestInfo, token);
    }

    /// <summary>
    /// 发送数据的切入点，该方法由框架自动调用。
    /// </summary>
    public Task SendInputAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        return this.PreviewSendAsync(memory, token);
    }

    /// <summary>
    /// 当发送数据前预先处理数据
    /// </summary>
    /// <param name="requestInfo"></param>
    /// <param name="token">可取消令箭</param>
    protected virtual async Task PreviewSendAsync(IRequestInfo requestInfo, CancellationToken token = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(requestInfo, nameof(requestInfo));

        var requestInfoBuilder = (IRequestInfoBuilder)requestInfo;

        var byteBlock = new ValueByteBlock(requestInfoBuilder.MaxLength);
        try
        {
            requestInfoBuilder.Build(ref byteBlock);
            await this.GoSendAsync(byteBlock.Memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 当发送数据前预先处理数据
    /// </summary>
    protected virtual Task PreviewSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        return this.GoSendAsync(memory, token);
    }
    #endregion SendInput

    /// <summary>
    /// 处理已经经过预先处理后的数据
    /// </summary>
    /// <param name="reader">以二进制形式传递</param>
    /// <param name="requestInfo">以解析实例传递</param>
    protected virtual Task GoReceivedAsync(IByteBlockReader reader, IRequestInfo requestInfo)
    {
        return this.ReceivedAsyncCallBack == null ? EasyTask.CompletedTask : this.ReceivedAsyncCallBack.Invoke(reader, requestInfo);
    }

    /// <summary>
    /// 异步发送已经经过预先处理后的数据
    /// </summary>
    /// <param name="memory">数据</param>
    /// <param name="token">可取消令箭</param>
    /// <returns></returns>
    protected virtual Task GoSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        return this.SendAsyncCallBack == null ? EasyTask.CompletedTask : this.SendAsyncCallBack.Invoke(memory, token);
    }

    /// <summary>
    /// 当接收到数据后预先处理数据,然后调用<see cref="GoReceivedAsync(IByteBlockReader, IRequestInfo)"/>处理数据
    /// </summary>
    /// <param name="reader"></param>
    protected abstract Task PreviewReceivedAsync(IByteBlockReader reader);

    /// <summary>
    /// 重置解析器到初始状态，一般在<see cref="DataHandlingAdapter.OnError(Exception,string, bool, bool)"/>被触发时，由返回值指示是否调用。
    /// </summary>
    protected override void Reset()
    {
        this.LastCacheTime = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {

    }
}