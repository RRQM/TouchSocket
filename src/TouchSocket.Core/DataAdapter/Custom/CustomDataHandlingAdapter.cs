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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 用户自定义数据处理适配器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为<see langword="null"/>，
/// 同时<see cref="IRequestInfo"/>将实现为TRequest，发送数据直接发送。
/// </summary>
public abstract class CustomDataHandlingAdapter<TRequest> : SingleStreamDataHandlingAdapter
    where TRequest : IRequestInfo
{
    private TRequest m_tempRequest;

    #region ParseRequest
    /// <summary>
    /// 尝试从字节读取器中解析出请求信息。
    /// </summary>
    /// <typeparam name="TReader">字节读取器类型。</typeparam>
    /// <param name="reader">字节读取器的引用。</param>
    /// <param name="request">解析出的请求信息。</param>
    /// <returns>解析成功返回 true，否则返回 false。</returns>
    public bool TryParseRequest<TReader>(ref TReader reader, out TRequest request)
        where TReader : IBytesReader
    {
        this.CacheVerify(ref reader);
        return this.ParseRequestCore(ref reader, out request);
    }

    protected virtual bool ParseRequestCore<TReader>(ref TReader reader, out TRequest request)
       where TReader : IBytesReader
    {
        var result = this.Filter(ref reader, this.IsBeCached(this.m_tempRequest), ref this.m_tempRequest);
        switch (result)
        {
            case FilterResult.Success:
                {
                    request = this.m_tempRequest;
                    this.m_tempRequest = default;
                    return true;
                }
            case FilterResult.Cache:
                {
                    request = this.m_tempRequest;
                    return false;
                }
            case FilterResult.GoOn:
            default:
                {
                    return this.ParseRequestCore(ref reader, out request);
                }
        }
    }

    #endregion

    /// <inheritdoc/>
    public override bool CanSendRequestInfo => false;

    /// <summary>
    /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
    /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLength"/>的数据</para>
    /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Position"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
    /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Position"/>移至指定位置。</para>
    /// </summary>
    /// <param name="reader">字节块</param>
    /// <param name="beCached">是否为上次遗留对象，当该参数为<see langword="true"/>时，request也将是上次实例化的对象。</param>
    /// <param name="request">对象。</param>
    /// <returns></returns>
    protected abstract FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref TRequest request)
        where TReader : IBytesReader;

    /// <summary>
    /// 判断请求对象是否应该被缓存。
    /// </summary>
    /// <param name="request">请求对象。</param>
    /// <returns>返回布尔值，指示请求对象是否应该被缓存。</returns>
    protected virtual bool IsBeCached(in TRequest request)
    {
        if (typeof(TRequest).IsValueType)
        {
            // 判断值类型是否为默认值
            return !EqualityComparer<TRequest>.Default.Equals(request, default);
        }
        else
        {
            // 判断引用类型是否为null
            return request != null;
        }
    }

    /// <inheritdoc/>
    protected override async Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            if (this.DisposedValue)
            {
                return;
            }
            var filterResult = this.Filter(ref reader, this.IsBeCached(this.m_tempRequest), ref this.m_tempRequest);

            switch (filterResult)
            {
                case FilterResult.Success:
                    {
                        await this.GoReceivedAsync(null, this.m_tempRequest).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        this.m_tempRequest = default;
                        break;
                    }
                case FilterResult.Cache:
                    return;//缓存数据，等待下次接收
                case FilterResult.GoOn:
                    break;//继续处理数据
            }
        }
    }

    /// <inheritdoc/>
    protected override void Reset()
    {
        this.m_tempRequest = default;
        base.Reset();
    }
}