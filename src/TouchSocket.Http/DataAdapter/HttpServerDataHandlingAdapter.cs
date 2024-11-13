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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http服务器数据处理适配器
    /// </summary>
    public sealed class HttpServerDataHandlingAdapter : SingleStreamDataHandlingAdapter
    {
        private HttpRequest m_request;
        private HttpRequest m_requestRoot;
        private long m_surLen;
        private Task m_task;
        private ByteBlock m_tempByteBlock;

        /// <inheritdoc/>
        public override bool CanSplicingSend => false;

        /// <inheritdoc/>
        public override void OnLoaded(object owner)
        {
            if (owner is not HttpSessionClient httpSessionClient)
            {
                throw new Exception($"此适配器必须适用于{nameof(IHttpService)}");
            }

            this.m_requestRoot = new HttpRequest(httpSessionClient);
            base.OnLoaded(owner);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //this.m_requestRoot.SafeDispose();
                this.m_tempByteBlock.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        /// <param name="byteBlock"></param>
        protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
        {
            if (this.m_tempByteBlock == null)
            {
                byteBlock.Position = 0;
                await this.Single(byteBlock, false).ConfigureAwait(false);
            }
            else
            {
                this.m_tempByteBlock.Write(byteBlock.Span);
                var block = this.m_tempByteBlock;
                this.m_tempByteBlock = null;
                block.Position = 0;
                await this.Single(block, true).ConfigureAwait(false);
            }
        }

        private void Cache(ByteBlock byteBlock)
        {
            if (byteBlock.CanReadLength > 0)
            {
                this.m_tempByteBlock = new ByteBlock();
                this.m_tempByteBlock.Write(byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength));
                if (this.m_tempByteBlock.Length > this.MaxPackageSize)
                {
                    this.OnError(default, "缓存的数据长度大于设定值的情况下未收到解析信号", true, true);
                }
            }
        }

        private async Task DestoryRequest()
        {
            this.m_request = null;
            if (this.m_task != null)
            {
                await this.m_task.ConfigureAwait(false);
                this.m_task = null;
            }
        }

        private async Task Single(ByteBlock byteBlock, bool dis)
        {
            try
            {
                while (byteBlock.CanReadLength > 0)
                {
                    if (this.DisposedValue)
                    {
                        return;
                    }
                    if (this.m_request == null)
                    {
                        this.m_requestRoot.ResetHttp();
                        this.m_request = this.m_requestRoot;
                        if (this.m_request.ParsingHeader(ref byteBlock))
                        {
                            //byteBlock.Position++;
                            if (this.m_request.ContentLength > byteBlock.CanReadLength)
                            {
                                this.m_surLen = this.m_request.ContentLength;

                                this.m_task = this.TaskRunGoReceived(this.m_request);
                            }
                            else
                            {
                                this.m_request.InternalSetContent(byteBlock.ReadToSpan((int)this.m_request.ContentLength).ToArray());
                                await this.GoReceivedAsync(null, this.m_request).ConfigureAwait(false);
                                await this.DestoryRequest().ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            this.Cache(byteBlock);
                            this.m_request = null;
                            return;
                        }
                    }

                    if (this.m_surLen > 0)
                    {
                        if (byteBlock.CanRead)
                        {
                            var len = (int)Math.Min(this.m_surLen, byteBlock.CanReadLength);

                            await this.m_request.InternalInputAsync(byteBlock.Memory.Slice(byteBlock.Position, len)).ConfigureAwait(false);
                            this.m_surLen -= len;
                            byteBlock.Position += len;
                            if (this.m_surLen == 0)
                            {
                                await this.m_request.CompleteInput().ConfigureAwait(false);
                                await this.DestoryRequest().ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        await this.DestoryRequest().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                if (dis)
                {
                    byteBlock.Dispose();
                }
            }
        }

        private Task TaskRunGoReceived(HttpRequest request)
        {
            var task = Task.Run(async () => await this.GoReceivedAsync(null, request).ConfigureAwait(false));
            task.ConfigureAwait(false);
            return task;
        }
    }
}