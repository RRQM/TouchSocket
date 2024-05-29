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

        ///// <summary>
        ///// 当接收数据处理完成后，回调该函数执行发送
        ///// </summary>
        //public Action<EndPoint, byte[], int, int> SendCallBack { get; set; }

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
                await this.PreviewReceived(remoteEndPoint, byteBlock).ConfigureFalseAwait();
            }
            catch (Exception ex)
            {
                this.OnError(ex, ex.Message, true, true);
            }
        }

        //#region SendInput

        ///// <summary>
        ///// 发送数据的切入点，该方法由框架自动调用。
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        //public void SendInput(EndPoint endPoint, byte[] buffer, int offset, int length)
        //{
        //    this.PreviewSend(endPoint, buffer, offset, length);
        //}

        ///// <summary>
        ///// 发送数据的切入点，该方法由框架自动调用。
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="requestInfo"></param>
        //public void SendInput(EndPoint endPoint, IRequestInfo requestInfo)
        //{
        //    this.PreviewSend(endPoint, requestInfo);
        //}

        ///// <summary>
        ///// 发送数据的切入点，该方法由框架自动调用。
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="transferBytes"></param>
        //public void SendInput(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        //{
        //    this.PreviewSend(endPoint, transferBytes);
        //}

        //#endregion SendInput

        #region SendInputAsync

        /// <summary>
        /// 发送数据的切入点，该方法由框架自动调用。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
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
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock">以二进制形式传递</param>
        /// <param name="requestInfo">以解析实例传递</param>
        protected Task GoReceived(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            return this.ReceivedCallBack.Invoke(remoteEndPoint, byteBlock, requestInfo);
        }

        ///// <summary>
        ///// 发送已经经过预先处理后的数据
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        //protected void GoSend(EndPoint endPoint, byte[] buffer, int offset, int length)
        //{
        //    this.SendCallBack.Invoke(endPoint, buffer, offset, length);
        //}

        /// <summary>
        /// 发送已经经过预先处理后的数据
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
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
            await this.GoReceived(remoteEndPoint, byteBlock, default).ConfigureFalseAwait();
        }

        ///// <summary>
        ///// 当发送数据前预先处理数据
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="requestInfo"></param>
        //protected virtual void PreviewSend(EndPoint endPoint, IRequestInfo requestInfo)
        //{
        //    ThrowHelper.ThrowArgumentNullExceptionIf(requestInfo,nameof(requestInfo));

        //    var requestInfoBuilder = (IRequestInfoBuilder)requestInfo;
        //    using (var byteBlock = new ByteBlock(requestInfoBuilder.MaxLength))
        //    {
        //        requestInfoBuilder.Build(byteBlock);
        //        this.GoSend(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
        //    }
        //}

        ///// <summary>
        ///// 当发送数据前预先处理数据
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="buffer">数据</param>
        ///// <param name="offset">偏移</param>
        ///// <param name="length">长度</param>
        //protected virtual void PreviewSend(EndPoint endPoint, byte[] buffer, int offset, int length)
        //{
        //    this.GoSend(endPoint, buffer, offset, length);
        //}

        ///// <summary>
        ///// 组合发送预处理数据，
        ///// 当属性SplicingSend实现为True时，系统才会调用该方法。
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="transferBytes">代发送数据组合</param>
        //protected virtual void PreviewSend(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        //{
        //    throw new NotImplementedException();
        //}

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
                await this.GoSendAsync(endPoint, byteBlock.Memory).ConfigureFalseAwait();
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 当发送数据前预先处理数据
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected virtual Task PreviewSendAsync(EndPoint endPoint,ReadOnlyMemory<byte> memory)
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