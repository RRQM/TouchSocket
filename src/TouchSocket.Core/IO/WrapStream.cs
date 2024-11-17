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

using System.IO;

namespace TouchSocket.Core
{
    /// <summary>
    /// 包装的流。为避免该流释放时，内部流也会被释放的问题
    /// </summary>

    public partial class WrapStream : Stream
    {
        private readonly Stream m_stream;

        /// <summary>
        /// 包装的流。为避免该流释放时，内部流也会被释放的问题
        /// </summary>
        /// <param name="stream"></param>
        public WrapStream(Stream stream)
        {
            this.m_stream = stream;
        }

        /// <inheritdoc/>
        public override bool CanRead => this.m_stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => this.m_stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => this.m_stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => this.m_stream.Length;

        /// <inheritdoc/>
        public override long Position { get => this.m_stream.Position; set => this.m_stream.Position = value; }

        /// <inheritdoc/>

        public override void Flush()
        {
            this.m_stream.Flush();
        }

        /// <inheritdoc/>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.m_stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.m_stream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            this.m_stream.SetLength(value);
        }

        /// <inheritdoc/>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.m_stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// 没有关闭效果
        /// </summary>
        public override void Close()
        {
        }

        /// <summary>
        /// 没有释放效果
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
        }
    }
}