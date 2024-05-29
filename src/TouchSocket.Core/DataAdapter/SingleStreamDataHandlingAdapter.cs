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
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
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
        /// 是否启用缓存超时。默认false。
        /// </summary>
        public bool CacheTimeoutEnable { get; set; } = false;

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => false;

        /// <inheritdoc/>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// 当接收数据处理完成后，回调该函数执行接收
        /// </summary>
        public Func<ByteBlock, IRequestInfo, Task> ReceivedAsyncCallBack { get; set; }

        /// <summary>
        /// 当发送数据处理完成后，回调该函数执行异步发送
        /// </summary>
        public Func<ReadOnlyMemory<byte>, Task> SendAsyncCallBack { get; set; }

        /// <summary>
        /// 是否在收到数据时，即刷新缓存时间。默认true。
        /// <list type="number">
        /// <item>当设为true时，将弱化<see cref="CacheTimeout"/>的作用，只要一直有数据，则缓存不会过期。</item>
        /// <item>当设为false时，则在<see cref="CacheTimeout"/>的时效内。必须完成单个缓存的数据。</item>
        /// </list>
        /// </summary>
        public bool UpdateCacheTimeWhenRev { get; set; } = true;

        /// <summary>
        /// 最后缓存的时间
        /// </summary>
        protected DateTime LastCacheTime { get; set; }

        /// <summary>
        /// 收到数据的切入点，该方法由框架自动调用。
        /// </summary>
        /// <param name="byteBlock"></param>
        public async Task ReceivedInputAsync(ByteBlock byteBlock)
        {
            try
            {
                await this.PreviewReceivedAsync(byteBlock).ConfigureFalseAwait();
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
        /// <returns></returns>
        public Task SendInputAsync(IRequestInfo requestInfo)
        {
            return this.PreviewSendAsync(requestInfo);
        }

        /// <summary>
        /// 发送数据的切入点，该方法由框架自动调用。
        /// </summary>
        public Task SendInputAsync(ReadOnlyMemory<byte> memory)
        {
            return this.PreviewSendAsync(memory);
        }

        /// <summary>
        /// 发送数据的切入点，该方法由框架自动调用。
        /// </summary>
        /// <param name="transferBytes"></param>
        public Task SendInputAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return this.PreviewSendAsync(transferBytes);
        }

        /// <summary>
        /// 当发送数据前预先处理数据
        /// </summary>
        /// <param name="requestInfo"></param>
        protected virtual async Task PreviewSendAsync(IRequestInfo requestInfo)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(requestInfo,nameof(requestInfo));

            var requestInfoBuilder = (IRequestInfoBuilder)requestInfo;

            var byteBlock = new ByteBlock(requestInfoBuilder.MaxLength);
            try
            {
                requestInfoBuilder.Build(ref byteBlock);
                await this.GoSendAsync(byteBlock.Memory).ConfigureFalseAwait();
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 当发送数据前预先处理数据
        /// </summary>
        protected virtual Task PreviewSendAsync(ReadOnlyMemory<byte> memory)
        {
            return this.GoSendAsync(memory);
        }

        /// <summary>
        /// 组合发送预处理数据，
        /// 当属性SplicingSend实现为True时，系统才会调用该方法。
        /// </summary>
        /// <param name="transferBytes">代发送数据组合</param>
        protected virtual Task PreviewSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            throw new NotImplementedException();
        }

        #endregion SendInput

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// 处理已经经过预先处理后的数据
        /// </summary>
        /// <param name="byteBlock">以二进制形式传递</param>
        /// <param name="requestInfo">以解析实例传递</param>
        protected virtual Task GoReceivedAsync(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.ReceivedAsyncCallBack == null)
            {
                return EasyTask.CompletedTask;
            }
            return this.ReceivedAsyncCallBack.Invoke(byteBlock, requestInfo);
        }

        /// <summary>
        /// 异步发送已经经过预先处理后的数据
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        protected virtual Task GoSendAsync(ReadOnlyMemory<byte> memory)
        {
            if (this.SendAsyncCallBack == null)
            {
                return EasyTask.CompletedTask;
            }
            return this.SendAsyncCallBack.Invoke(memory);
        }

        /// <summary>
        /// 当接收到数据后预先处理数据,然后调用<see cref="GoReceivedAsync(ByteBlock, IRequestInfo)"/>处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected abstract Task PreviewReceivedAsync(ByteBlock byteBlock);

        /// <summary>
        /// 重置解析器到初始状态，一般在<see cref="DataHandlingAdapter.OnError(Exception,string, bool, bool)"/>被触发时，由返回值指示是否调用。
        /// </summary>
        protected override void Reset()
        {
            this.LastCacheTime = DateTime.Now;
        }
    }
}