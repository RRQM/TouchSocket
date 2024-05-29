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
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http客户端数据处理适配器
    /// </summary>
    public sealed class HttpClientDataHandlingAdapter : SingleStreamDataHandlingAdapter
    {
        private ByteBlock m_tempByteBlock;
        private HttpResponse m_httpResponse;
        private HttpResponse m_httpResponseRoot;

        private long m_surLen;

        private Task m_task;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <inheritdoc/>
        public override void OnLoaded(object owner)
        {
            if (owner is not HttpClientBase clientBase)
            {
                throw new Exception($"此适配器必须适用于{nameof(HttpClientBase)}");
            }
            this.m_httpResponseRoot = new HttpResponse(clientBase);
            base.OnLoaded(owner);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
        {
            if (this.m_tempByteBlock == null)
            {
                byteBlock.Position = 0;
                await this.Single(byteBlock, false).ConfigureFalseAwait();
            }
            else
            {
                this.m_tempByteBlock.Write(byteBlock.Span);
                var block = this.m_tempByteBlock;
                this.m_tempByteBlock = null;
                block.Position = 0;
                await this.Single(block, true).ConfigureFalseAwait();
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

        private async Task<FilterResult> ReadChunk(ByteBlock byteBlock)
        {
            var position = byteBlock.Position;
            var index = byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength).IndexOf(TouchSocketHttpUtility.CRLF);
            if (index > 0)
            {
                var headerLength = index - byteBlock.Position;
                var hex = byteBlock.Span.Slice(byteBlock.Position, headerLength - 1).ToString(Encoding.UTF8);
                var count = hex.ByHexStringToInt32();
                byteBlock.Position += headerLength + 1;

                if (count >= 0)
                {
                    if (count > byteBlock.CanReadLength)
                    {
                        byteBlock.Position = position;
                        return FilterResult.Cache;
                    }

                    await this.m_httpResponse.InternalInputAsync(byteBlock.Memory.Slice(byteBlock.Position, count)).ConfigureFalseAwait();
                    byteBlock.Position += count;
                    byteBlock.Position += 2;
                    return FilterResult.GoOn;
                }
                else
                {
                    byteBlock.Position += 2;
                    return FilterResult.Success;
                }
            }
            else
            {
                return FilterResult.Cache;
            }
        }

        private Task RunGoReceived(HttpResponse response)
        {
            return Task.Run(() => this.GoReceivedAsync(null, response));
        }

        private async Task Single(ByteBlock byteBlock, bool dis)
        {
            try
            {
                while (byteBlock.CanReadLength > 0)
                {
                    if (this.m_httpResponse == null)
                    {
                        this.m_httpResponseRoot.ResetHttp();
                        this.m_httpResponse = this.m_httpResponseRoot;
                        if (this.m_httpResponse.ParsingHeader(byteBlock, byteBlock.CanReadLength))
                        {
                            //byteBlock.Position++;
                            if (this.m_httpResponse.IsChunk || this.m_httpResponse.ContentLength > byteBlock.CanReadLength)
                            {
                                this.m_surLen = this.m_httpResponse.ContentLength;
                                this.m_task = this.RunGoReceived(this.m_httpResponse);
                            }
                            else
                            {
                                byteBlock.Read(out var buffer, (int)this.m_httpResponse.ContentLength);
                                this.m_httpResponse.SetContent(buffer);
                                await this.GoReceivedAsync(null, this.m_httpResponse).ConfigureFalseAwait();
                                this.m_httpResponse = null;
                            }
                        }
                        else
                        {
                            this.Cache(byteBlock);
                            this.m_httpResponse = null;

                            if (this.m_task != null)
                            {
                                await this.m_task.ConfigureFalseAwait();
                                this.m_task = null;
                            }
                            return;
                        }
                    }
                    else
                    {
                        if (this.m_httpResponse.IsChunk)
                        {
                            switch (await this.ReadChunk(byteBlock).ConfigureFalseAwait())
                            {
                                case FilterResult.Cache:
                                    this.Cache(byteBlock);
                                    return;

                                case FilterResult.Success:
                                    this.m_httpResponse = null;
                                    if (this.m_task != null)
                                    {
                                        await this.m_task;
                                        this.m_task = null;
                                    }
                                    break;

                                case FilterResult.GoOn:
                                default:
                                    break;
                            }
                        }
                        else if (this.m_surLen > 0)
                        {
                            if (byteBlock.CanRead)
                            {
                                var len = (int)Math.Min(this.m_surLen, byteBlock.CanReadLength);
                                await this.m_httpResponse.InternalInputAsync(byteBlock.Memory.Slice(byteBlock.Position, len)).ConfigureFalseAwait();
                                this.m_surLen -= len;
                                byteBlock.Position += len;
                                if (this.m_surLen == 0)
                                {
                                    await this.m_httpResponse.CompleteInput().ConfigureFalseAwait();
                                    this.m_httpResponse = null;
                                    if (this.m_task != null)
                                    {
                                        await this.m_task.ConfigureFalseAwait();
                                        this.m_task = null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            this.m_httpResponse = null;
                            if (this.m_task != null)
                            {
                                await this.m_task.ConfigureFalseAwait();
                                this.m_task = null;
                            }
                        }
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
    }
}