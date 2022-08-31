using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.IO;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 管道数据处理适配器。
    /// 使用该适配器后，<see cref="IRequestInfo"/>将为<see cref="Pipeline"/>.
    /// </summary>
    public class PipelineDataHandlingAdapter : NormalDataHandlingAdapter
    {
        private InternalPipeline m_pipeline;


        /// <summary>
        /// 管道数据处理适配器。
        /// 使用该适配器后，<see cref="IRequestInfo"/>将为<see cref="Pipeline"/>.
        /// </summary>
        public PipelineDataHandlingAdapter()
        {
        }
        byte[] m_buffer;
        Task m_task;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (this.m_pipeline == null || !this.m_pipeline.Enable)
            {
                this.m_task?.GetAwaiter().GetResult();
                this.m_pipeline = new InternalPipeline(this.Client);
                m_task = Task.Run(() =>
                  {
                      try
                      {
                          this.GoReceived(default, this.m_pipeline);
                          if (this.m_pipeline.CanReadLen>0)
                          {
                              this.m_buffer = new byte[this.m_pipeline.CanReadLen];
                              this.m_pipeline.Read(this.m_buffer, 0, this.m_buffer.Length);
                          }
                      }
                      catch
                      {

                      }
                      finally
                      {
                          this.m_pipeline.SafeDispose();
                      }
                  });
            }
            if (this.m_buffer != null)
            {
                this.m_pipeline.InternalInput(this.m_buffer, 0, this.m_buffer.Length);
                this.m_buffer = null;
            }
            this.m_pipeline.InternalInput(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_pipeline.SafeDispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Pipeline读取管道
    /// </summary>
    public abstract class Pipeline : BlockReadStream, IRequestInfo
    {
        /// <summary>
        /// 当前支持此管道的客户端。
        /// </summary>
        public ITcpClientBase Client { get; set; }

        /// <summary>
        /// Pipeline读取管道
        /// </summary>
        /// <param name="client"></param>
        protected Pipeline(ITcpClientBase client)
        {
            this.Client = client;
        }
    }

    internal class InternalPipeline : Pipeline
    {
        /// <summary>
        /// Pipeline读取管道
        /// </summary>
        /// <param name="client"></param>
        public InternalPipeline(ITcpClientBase client) : base(client)
        {
            this.ReadTimeout = 60 * 1000;
        }
        private readonly object m_locker = new object();
        private bool m_disposedValue;

        public bool Enable
        {
            get
            {
                lock (this.m_locker)
                {
                    return !this.m_disposedValue;
                }
            }
        }

        public override bool CanWrite => this.Client.CanSend;

        public override bool CanRead => this.Enable;

        internal void InternalInput(byte[] buffer, int offset, int length)
        {
            Input(buffer, offset, length);
        }

        protected override void Dispose(bool disposing)
        {
            lock (this.m_locker)
            {
                this.m_disposedValue = true;
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.Client.DefaultSend(buffer, offset, count);
        }
    }
}
