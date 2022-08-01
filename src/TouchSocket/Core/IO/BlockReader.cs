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
using System.Threading;

namespace TouchSocket.Core.IO
{
    /// <summary>
    /// 阻塞式读取。
    /// </summary>
    public abstract class BlockReader : DisposableObject
    {
        private byte[] m_buffer;
        private readonly AutoResetEvent m_inputEvent;
        private volatile int m_offset;
        private readonly AutoResetEvent m_readEvent;
        private volatile int m_surLength;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BlockReader()
        {
            this.m_readEvent = new AutoResetEvent(false);
            this.m_inputEvent = new AutoResetEvent(false);
            this.ReadTimeout = 5000;
        }

        /// <summary>
        /// 可读
        /// </summary>
        public abstract bool CanRead { get; }

        /// <summary>
        /// 可写
        /// </summary>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int ReadTimeout { get; set; }

        /// <summary>
        /// 阻塞读取。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual int Read(byte[] buffer, int offset, int count)
        {
            if (!this.CanRead)
            {
                throw new Exception("该流不允许读取。");
            }
            int r;
            if (this.m_surLength > 0)
            {
                if (this.m_surLength > count)
                {
                    //按count读取
                    Array.Copy(this.m_buffer, this.m_offset, buffer, offset, count);
                    this.m_surLength -= count;
                    this.m_offset += count;
                    r = count;
                }
                else
                {
                    //会读完本次
                    Array.Copy(this.m_buffer, this.m_offset, buffer, offset, this.m_surLength);
                    r = this.m_surLength;
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
                        Array.Copy(this.m_buffer, this.m_offset, buffer, offset, count);
                        this.m_surLength -= count;
                        this.m_offset += count;
                        r = count;
                    }
                    else
                    {
                        //会读完本次
                        Array.Copy(this.m_buffer, this.m_offset, buffer, offset, this.m_surLength);
                        r = this.m_surLength;
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
        /// 传输输入.
        /// 必须以length为0结束。读取端会超时。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected bool Input(byte[] buffer, int offset, int length)
        {
            //if (this.disposedValue)
            //{
            //    return false;
            //}
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
            this.m_offset = 0;
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
            this.m_readEvent.SafeDispose();
            this.m_inputEvent.SafeDispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~BlockReader()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: false);
        }
    }
}