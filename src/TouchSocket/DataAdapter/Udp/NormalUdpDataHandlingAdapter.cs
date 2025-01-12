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
    /// 常规UDP数据处理适配器
    /// </summary>
    public class NormalUdpDataHandlingAdapter : UdpDataHandlingAdapter
    {
        /// <inheritdoc/>
        public override bool CanSplicingSend => true;

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => false;

        /// <inheritdoc/>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override Task PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            return this.GoReceived(remoteEndPoint, byteBlock, null);
        }

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        //protected override void PreviewSend(EndPoint endPoint, byte[] buffer, int offset, int length)
        //{
        //    this.GoSend(endPoint, buffer, offset, length);
        //}

        ///// <inheritdoc/>
        //protected override void PreviewSend(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        //{
        //    var length = 0;
        //    foreach (var item in transferBytes)
        //    {
        //        length += item.Count;
        //    }

        //    this.ThrowIfMoreThanMaxPackageSize(length);

        //    using (var byteBlock = new ByteBlock(length))
        //    {
        //        foreach (var item in transferBytes)
        //        {
        //            byteBlock.Write(item.Array, item.Offset, item.Count);
        //        }
        //        this.GoSend(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
        //    }
        //}

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }

            this.ThrowIfMoreThanMaxPackageSize(length);

            using (var byteBlock = new ByteBlock(length))
            {
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
                }
                await this.GoSendAsync(endPoint, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
    }
}