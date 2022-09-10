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
using System.IO;

namespace TouchSocket.Core.IO
{
    /// <summary>
    /// FileStorageStream
    /// </summary>
    public class FileStorageStream : Stream
    {
        private readonly FileStorage m_fileStorage;
        private long m_position;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileStorage"></param>
        public FileStorageStream(FileStorage fileStorage)
        {
            this.m_fileStorage = fileStorage;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~FileStorageStream()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// 文件存储器
        /// </summary>
        public FileStorage FileStorage => this.m_fileStorage;


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanRead => this.m_fileStorage.FileStream.CanRead;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSeek => this.m_fileStorage.FileStream.CanSeek;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanWrite => this.m_fileStorage.FileStream.CanWrite;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override long Length => this.m_fileStorage.FileStream.Length;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override long Position { get => this.m_position; set => this.m_position = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Flush()
        {
            this.m_fileStorage.Flush();
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
            return this.m_fileStorage.Read(this.m_position, buffer, offset, count);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.m_position = offset;
                    break;

                case SeekOrigin.Current:
                    this.m_position += offset;
                    break;

                case SeekOrigin.End:
                    this.m_position = this.Length + offset;
                    break;
            }
            return this.m_position;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            this.m_fileStorage.FileStream.SetLength(value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.m_fileStorage.Write(this.m_position, buffer, offset, count);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            FilePool.TryReleaseFile(this.m_fileStorage?.Path);
            base.Dispose(disposing);
        }
    }
}
