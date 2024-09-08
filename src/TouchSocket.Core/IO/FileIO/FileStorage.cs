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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 文件存储器。在该存储器中，读写线程安全。
    /// </summary>
    public partial class FileStorage
    {
        internal volatile int m_reference;
        private readonly ReaderWriterLockSlim m_lockSlim;
        private bool m_disposedValue;

        /// <summary>
        /// 初始化一个文件存储器。在该存储器中，读写线程安全。
        /// </summary>
        internal FileStorage(FileInfo fileInfo, FileAccess fileAccess) : this()
        {
            this.FileAccess = fileAccess;
            this.FileInfo = fileInfo;
            this.Path = fileInfo.FullName;
            this.m_reference = 0;
            this.FileStream = fileInfo.Open(FileMode.OpenOrCreate, fileAccess, FileShare.ReadWrite);
            this.m_lockSlim = new ReaderWriterLockSlim();
        }

        private FileStorage()
        {
            this.AccessTime = DateTime.UtcNow;
            this.AccessTimeout = TimeSpan.FromSeconds(60);
        }

        /// <summary>
        /// 最后访问时间。
        /// </summary>
        public DateTime AccessTime { get; private set; }

        /// <summary>
        /// 访问超时时间。默认60s
        /// </summary>
        public TimeSpan AccessTimeout { get; set; }

        /// <summary>
        /// 访问属性
        /// </summary>
        public FileAccess FileAccess { get; private set; }

        /// <summary>
        /// 文件信息
        /// </summary>
        public FileInfo FileInfo { get; private set; }

        /// <summary>
        /// 文件流。
        /// 一般情况下，请不要直接访问该对象。否则有可能会产生不可预测的错误。
        /// </summary>
        public FileStream FileStream { get; private set; }

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length => this.FileStream.Length;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 引用次数。
        /// </summary>
        public int Reference => this.m_reference;

        /// <summary>
        /// 写入时清空缓存区
        /// </summary>
        public void Flush()
        {
            this.AccessTime = DateTime.UtcNow;
            this.FileStream.Flush();
        }

        /// <summary>
        /// 从当前文件中读取字节。
        /// </summary>
        /// <param name="startPos">开始读取的位置。</param>
        /// <param name="span">用于接收读取数据的字节跨度。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="ObjectDisposedException">如果当前对象已被处置。</exception>
        /// <exception cref="System.IO.IOException">如果文件仅被写入。</exception>
        public int Read(long startPos, Span<byte> span)
        {
            // 更新访问时间，用于跟踪文件的最近访问时间。
            this.AccessTime = DateTime.UtcNow;
            // 使用写锁保护共享资源，确保读操作的线程安全性。
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                // 检查对象是否已被处置，如果是，则抛出异常。
                if (this.m_disposedValue)
                {
                    ThrowHelper.ThrowObjectDisposedException(this);
                }
                // 检查文件访问模式，如果是写模式，则抛出异常。
                if (this.FileAccess == FileAccess.Write)
                {
                    ThrowHelper.ThrowException(TouchSocketCoreResource.FileOnlyWrittenTo.Format(this.FileInfo.FullName));
                }


                this.FileStream.Position = startPos;
                return this.FileStream.Read(span);
            }
        }

        /// <summary>
        /// 减少引用次数，并尝试释放流。
        /// </summary>
        /// <param name="delayTime">延迟释放时间。当设置为0时，立即释放,单位毫秒。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public Result TryReleaseFile(int delayTime = 0)
        {
            return FilePool.TryReleaseFile(this.Path, delayTime);
        }

        /// <summary>
        /// 写入数据到文件的特定位置。
        /// </summary>
        /// <param name="startPos">开始写入的位置。</param>
        /// <param name="span">要写入的字节跨度。</param>
        public void Write(long startPos, ReadOnlySpan<byte> span)
        {
            // 更新文件的访问时间。
            this.AccessTime = DateTime.UtcNow;

            // 使用写锁确保线程安全。
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                // 检查对象是否已释放。
                if (this.m_disposedValue)
                {
                    // 如果对象已释放，抛出ObjectDisposedException。
                    ThrowHelper.ThrowObjectDisposedException(this);
                }

                // 检查文件访问权限。
                if (this.FileAccess == FileAccess.Read)
                {
                    // 如果文件只读，抛出异常。
                    ThrowHelper.ThrowException(TouchSocketCoreResource.FileReadOnly.Format(this.FileInfo.FullName));
                }

                // 设置文件流的位置为指定的开始写入位置。
                this.FileStream.Position = startPos;

                // 将数据写入文件。
                this.FileStream.Write(span);
            }
        }

        internal void Dispose()
        {
            if (this.m_disposedValue)
            {
                return;
            }
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                this.m_disposedValue = true;
                this.FileStream.SafeDispose();
            }
        }
    }
}