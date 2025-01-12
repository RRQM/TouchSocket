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
using System.Net;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Udp数据处理适配器
    /// </summary>
    public abstract class UdpDataHandlingAdapter : DataHandlingAdapter
    {
        /// <inheritdoc/>
        public override bool CanSendRequestInfo => false;

        /// <inheritdoc/>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// 当接收数据处理完成后，回调该函数执行接收
        /// </summary>
        public Func<EndPoint, ByteBlock, IRequestInfo, Task> ReceivedCallBack { get; set; }

        /// <summary>
        /// 当接收数据处理完成后，异步回调该函数执行发送
        /// </summary>
        public Func<EndPoint, ReadOnlyMemory<byte>, Task> SendCallBackAsync { get; set; }

        /// <summary>
        /// 收到数据的切入点，该方法由框架自动调用。
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        public virtual async Task ReceivedInput(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            try
            {
                await this.PreviewReceived(remoteEndPoint, byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.OnError(ex, ex.Message, true, true);
            }
        }

        #region SendInputAsync

        /// <summary>
        /// 异步发送输入数据。
        /// </summary>
        /// <param name="endPoint">要发送数据的端点。</param>
        /// <param name="memory">包含要发送的数据的只读内存。</param>
        /// <returns>返回一个任务，表示发送操作。</returns>
        /// <remarks>
        /// 此方法是一个异步操作，用于向指定的端点发送输入数据。
        /// 它使用PreviewSendAsync方法来执行实际的发送操作。
        /// </remarks>
        public Task SendInputAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.PreviewSendAsync(endPoint, memory);
        }

        /// <summary>
        /// 发送数据的切入点，该方法由框架自动调用。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="requestInfo"></param>
        public Task SendInputAsync(EndPoint endPoint, IRequestInfo requestInfo)
        {
            return this.PreviewSendAsync(endPoint, requestInfo);
        }

        /// <summary>
        /// 发送数据的切入点，该方法由框架自动调用。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public Task SendInputAsync(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            return this.PreviewSendAsync(endPoint, transferBytes);
        }

        #endregion SendInputAsync

        /// <summary>
        /// 处理已经经过预先处理后的数据
        /// </summary>
        /// <param name="remoteEndPoint">远程端点，标识数据来源</param>
        /// <param name="byteBlock">接收到的二进制数据块</param>
        /// <param name="requestInfo">解析后的请求信息</param>
        /// <returns>一个异步任务，代表处理过程</returns>
        protected Task GoReceived(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            // 调用接收回调，继续处理接收到的数据
            return this.ReceivedCallBack.Invoke(remoteEndPoint, byteBlock, requestInfo);
        }

        /// <summary>
        /// 发送已经经过预先处理后的数据
        /// </summary>
        /// <param name="endPoint">目标端点，表示数据发送的目的地址</param>
        /// <param name="memory">已经经过预先处理的字节数据，以 ReadOnlyMemory 方式传递以提高性能</param>
        /// <returns>返回一个 Task 对象，表示异步操作的完成</returns>
        protected Task GoSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.SendCallBackAsync.Invoke(endPoint, memory);
        }
        /// <summary>
        /// 当接收到数据后预先处理数据,然后调用<see cref="GoReceived(EndPoint,ByteBlock, IRequestInfo)"/>处理数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected virtual async Task PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            await this.GoReceived(remoteEndPoint, byteBlock, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        /// <summary>
        /// 当发送数据前预先处理数据
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="requestInfo"></param>
        protected virtual async Task PreviewSendAsync(EndPoint endPoint, IRequestInfo requestInfo)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(requestInfo, nameof(requestInfo));

            var requestInfoBuilder = (IRequestInfoBuilder)requestInfo;

            var byteBlock = new ByteBlock(requestInfoBuilder.MaxLength);
            try
            {
                requestInfoBuilder.Build(ref byteBlock);
                await this.GoSendAsync(endPoint, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 当发送数据前预先处理数据。
        /// </summary>
        /// <param name="endPoint">数据发送的目标端点。</param>
        /// <param name="memory">待发送的字节数据内存。</param>
        /// <returns>一个表示异步操作完成的 <see cref="Task"/> 对象。</returns>
        protected virtual Task PreviewSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.GoSendAsync(endPoint, memory);
        }

        /// <summary>
        /// 组合发送预处理数据，
        /// 当属性SplicingSend实现为True时，系统才会调用该方法。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual Task PreviewSendAsync(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void Reset()
        {

        }
    }
}