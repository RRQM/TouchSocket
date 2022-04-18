using System;
using System.IO;
using System.Threading;

namespace RRQMCore.IO
{
    /// <summary>
    /// 阻塞式单项读取流。
    /// </summary>
    public abstract class BlockReadStream : Stream
    {
        private byte[] m_buffer;
        private AutoResetEvent m_inputEvent;
        private volatile int m_offset;
        private AutoResetEvent m_readEvent;
        private volatile int m_surLength;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BlockReadStream()
        {
            this.m_readEvent = new AutoResetEvent(false);
            this.m_inputEvent = new AutoResetEvent(false);
            this.ReadTimeout = 5000;
        }

        /// <summary>
        /// 可读
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// 不可使用
        /// </summary>
        public override bool CanSeek => throw new NotImplementedException();

        /// <summary>
        /// 不可使用
        /// </summary>
        public override long Length => throw new NotImplementedException();

        /// <summary>
        ///  不可使用
        /// </summary>
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int ReadTimeout { get; set; }

        /// <summary>
        /// 阻塞读取。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.CanRead)
            {
                throw new RRQMException("该流不允许读取。");
            }
            int r;
            if (this.m_surLength > 0)
            {
                if (this.m_surLength > count)
                {
                    //按count读取
                    Array.Copy(m_buffer, m_offset, buffer, offset, count);
                    m_surLength -= count;
                    m_offset += count;
                    r = count;
                }
                else
                {
                    //会读完本次
                    Array.Copy(m_buffer, m_offset, buffer, offset, m_surLength);
                    r = m_surLength;
                    this.Reset();
                }
            }
            else
            {
                //无数据，须等待
                if (this.m_readEvent.WaitOne(this.ReadTimeout))
                {
                    if (this.m_surLength == 0)
                    {
                        this.Reset();
                        r = 0;
                    }
                    else if (this.m_surLength > count)
                    {
                        //按count读取
                        Array.Copy(m_buffer, m_offset, buffer, offset, count);
                        m_surLength -= count;
                        m_offset += count;
                        r = count;
                    }
                    else
                    {
                        //会读完本次
                        Array.Copy(m_buffer, m_offset, buffer, offset, m_surLength);
                        r = m_surLength;
                        this.Reset();
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            return r;
        }

        /// <summary>
        /// 不可使用
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 不可使用
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 传输输入.
        /// 必须以length为0结束。读取端会超时。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected bool Input(byte[] buffer, int offset, int length)
        {
            this.m_inputEvent.Reset();
            this.m_buffer = buffer;
            this.m_offset = offset;
            this.m_surLength = length;
            this.m_readEvent.Set();
            return this.m_inputEvent.WaitOne(this.ReadTimeout);
        }

        private void Reset()
        {
            this.m_buffer = null;
            this.m_offset =0;
            this.m_surLength = 0;
            this.m_readEvent.Reset();
            this.m_inputEvent.Set();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Reset();
            this.m_readEvent.Dispose();
            this.m_inputEvent.Dispose();
            base.Dispose(disposing);
        }
    }
}