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
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpAdapter
    /// </summary>
    public class DmtpAdapter : CustomFixedHeaderByteBlockDataHandlingAdapter<DmtpMessage>
    {
        private readonly SemaphoreSlim m_locker = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => true;

        /// <inheritdoc/>
        public override bool CanSplicingSend => true;

        /// <inheritdoc/>
        public override int HeaderLength => 8;

        /// <summary>
        /// 最大拼接
        /// </summary>
        public const int MaxSplicing = 1024 * 64;

        /// <inheritdoc/>
        protected override DmtpMessage GetInstance()
        {
            return new DmtpMessage();
        }

        /// <inheritdoc/>
        protected override void OnReceivedSuccess(DmtpMessage request)
        {
            request.SafeDispose();
        }

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
        {
            if (!(requestInfo is DmtpMessage message))
            {
                throw new Exception($"无法将{nameof(requestInfo)}转换为{nameof(DmtpMessage)}");
            }
            if (message.BodyByteBlock != null && message.BodyByteBlock.Length > this.MaxPackageSize)
            {
                throw new Exception("发送的BodyLength={requestInfo.BodyLength},大于设定的MaxPackageSize={this.MaxPackageSize}");
            }
            using (var byteBlock = new ByteBlock(message.MaxLength))
            {
                message.Build(byteBlock);
                await this.GoSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        /// <inheritdoc/>
        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            if (!(requestInfo is DmtpMessage message))
            {
                throw new Exception($"无法将{nameof(requestInfo)}转换为{nameof(DmtpMessage)}");
            }
            if (message.BodyByteBlock != null && message.BodyByteBlock.Length > this.MaxPackageSize)
            {
                throw new Exception("发送的BodyLength={requestInfo.BodyLength},大于设定的MaxPackageSize={this.MaxPackageSize}");
            }
            using (var byteBlock = new ByteBlock(message.MaxLength))
            {
                message.Build(byteBlock);
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            if (transferBytes.Count == 0)
            {
                return;
            }

            try
            {
                await this.m_locker.WaitAsync();
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }

                if (length > this.MaxPackageSize)
                {
                    throw new Exception("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
                }
                if (length > MaxSplicing)
                {
                    foreach (var item in transferBytes)
                    {
                        await this.GoSendAsync(item.Array, item.Offset, item.Count);
                    }
                }
                else
                {
                    using (var byteBlock = new ByteBlock(length))
                    {
                        foreach (var item in transferBytes)
                        {
                            byteBlock.Write(item.Array, item.Offset, item.Count);
                        }
                        await this.GoSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
                    }
                }
            }
            finally
            {
                this.m_locker.Release();
            }
        }

        /// <inheritdoc/>
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        {
            if (transferBytes.Count == 0)
            {
                return;
            }
            try
            {
                this.m_locker.Wait();
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }

                if (length > this.MaxPackageSize)
                {
                    throw new Exception("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
                }

                if (length > MaxSplicing)
                {
                    foreach (var item in transferBytes)
                    {
                        this.GoSend(item.Array, item.Offset, item.Count);
                    }
                }
                else
                {
                    using (var byteBlock = new ByteBlock(length))
                    {
                        foreach (var item in transferBytes)
                        {
                            byteBlock.Write(item.Array, item.Offset, item.Count);
                        }
                        this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
                    }
                }
            }
            finally
            {
                this.m_locker.Release();
            }
        }
    }
}