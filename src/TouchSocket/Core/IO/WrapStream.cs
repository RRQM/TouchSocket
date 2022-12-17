using System.IO;

namespace TouchSocket.Core
{
    /// <summary>
    /// 包装的流。为避免该流释放时，内部流也会被释放的问题
    /// </summary>
    public class WrapStream : Stream
    {
        private readonly Stream m_stream;

        /// <summary>
        /// 包装的流。为避免该流释放时，内部流也会被释放的问题
        /// </summary>
        /// <param name="stream"></param>
        public WrapStream(Stream stream)
        {
            m_stream = stream;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanRead => m_stream.CanRead;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSeek => m_stream.CanSeek;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanWrite => m_stream.CanWrite;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override long Length => m_stream.Length;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override long Position { get => m_stream.Position; set => m_stream.Position = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Flush()
        {
            m_stream.Flush();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_stream.Seek(offset, origin);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            m_stream.SetLength(value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            m_stream.Write(buffer, offset, count);
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