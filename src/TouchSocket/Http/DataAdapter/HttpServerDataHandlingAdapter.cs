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
using System;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Run;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http服务器数据处理适配器
    /// </summary>
    public class HttpServerDataHandlingAdapter : NormalDataHandlingAdapter
    {
        /// <summary>
        /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="ByteBlock.Dispose"/>后，再置空；
        /// </summary>
        protected ByteBlock tempByteBlock;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (this.tempByteBlock == null)
            {
                byteBlock.Pos = 0;
                this.Single(byteBlock, false);
            }
            else
            {
                this.tempByteBlock.Write(byteBlock.Buffer, 0, byteBlock.Len);
                ByteBlock block = this.tempByteBlock;
                this.tempByteBlock = null;
                block.Pos = 0;
                this.Single(block, true);
            }
        }

        private HttpRequest m_httpRequest;
        private Task m_task;
        private long m_surLen;

        private void Single(ByteBlock byteBlock, bool dis)
        {
            try
            {
                while (byteBlock.CanReadLen > 0)
                {
                    if (this.m_httpRequest == null)
                    {
                        this.m_httpRequest = new HttpRequest(this.Client, true);
                        if (this.m_httpRequest.ParsingHeader(byteBlock, byteBlock.CanReadLen))
                        {
                            byteBlock.Pos++;
                            if (this.m_httpRequest.ContentLength > byteBlock.CanReadLength)
                            {
                                this.m_surLen = this.m_httpRequest.ContentLength;

                                this.m_task = EasyAction.TaskRun(this.m_httpRequest, (res) =>
                                {
                                    this.GoReceived(null, res);
                                });
                            }
                            else
                            {
                                byteBlock.Read(out byte[] buffer, (int)this.m_httpRequest.ContentLength);
                                this.m_httpRequest.SetContent(buffer);
                                this.GoReceived(null, this.m_httpRequest);
                                this.m_httpRequest = null;
                            }
                        }
                        else
                        {
                            this.Cache(byteBlock);
                            this.m_httpRequest = null;
                            this.m_task?.Wait();
                            this.m_task = null;
                            return;
                        }
                    }

                    if (this.m_surLen > 0)
                    {
                        if (byteBlock.CanRead)
                        {
                            int len = (int)Math.Min(this.m_surLen, byteBlock.CanReadLength);
                            this.m_httpRequest.InternalInput(byteBlock.Buffer, byteBlock.Pos, len);
                            this.m_surLen -= len;
                            byteBlock.Pos += len;
                            if (this.m_surLen == 0)
                            {
                                this.m_httpRequest.InternalInput(null, 0, 0);
                                this.m_httpRequest = null;
                                this.m_task?.Wait();
                                this.m_task = null;
                            }
                        }
                    }
                    else
                    {
                        this.m_httpRequest = null;
                        this.m_task?.Wait();
                        this.m_task = null;
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

        private void Cache(ByteBlock byteBlock)
        {
            if (byteBlock.CanReadLen > 0)
            {
                this.tempByteBlock = new ByteBlock();
                this.tempByteBlock.Write(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);
                if (this.tempByteBlock.Len > this.MaxPackageSize)
                {
                    this.OnError("缓存的数据长度大于设定值的情况下未收到解析信号");
                }
            }
        }
    }
}