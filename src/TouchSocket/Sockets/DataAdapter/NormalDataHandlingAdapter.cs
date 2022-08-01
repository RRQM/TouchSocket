//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections.Generic;
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 普通TCP数据处理器，该适配器不对数据做任何处理。
    /// </summary>
    public class NormalDataHandlingAdapter : DataHandlingAdapter
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// 当接收到数据时处理数据
        /// </summary>
        /// <param name="byteBlock">数据流</param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            this.GoReceived(byteBlock, null);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            this.GoSend(buffer, offset, length, isAsync);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(IList<TransferByte> transferBytes, bool isAsync)
        {
            throw new System.NotImplementedException();//因为设置了不支持拼接发送，所以该方法可以不实现。
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(IRequestInfo requestInfo, bool isAsync)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
        }
    }
}