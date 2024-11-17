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
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 文件读取器
    /// </summary>
    public partial class FileStorageReader : SafetyDisposableObject
    {
        private long m_position;

        /// <summary>
        /// 初始化FileStorageReader实例。
        /// </summary>
        /// <param name="fileStorage">文件存储对象，用于处理文件读取操作。</param>
        /// <remarks>
        /// 此构造函数接收一个FileStorage对象作为参数，用于后续的文件读取操作。
        /// 如果传入的FileStorage对象为null，则抛出ArgumentNullException异常，
        /// 这确保了文件读取操作不会在不合法的参数状态下执行，提高了代码的健壮性。
        /// </remarks>
        public FileStorageReader(FileStorage fileStorage)
        {
            // 使用ThrowHelper提供的扩展方法验证fileStorage参数是否为null，
            // 并将fileStorage赋值给类成员变量FileStorage。
            this.FileStorage = ThrowHelper.ThrowArgumentNullExceptionIf(fileStorage, nameof(fileStorage));
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~FileStorageReader()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// 文件存储器
        /// </summary>
        public FileStorage FileStorage { get; private set; }

        /// <summary>
        /// 游标位置
        /// </summary>
        public long Position { get => m_position; set => m_position = value; }

        /// <summary>
        /// 从文件存储中读取字节到指定的字节跨度。
        /// </summary>
        /// <param name="span">要读取数据的字节跨度。</param>
        /// <returns>实际读取到的字节数。</returns>
        public int Read(Span<byte> span)
        {
            // 调用文件存储从当前位置读取数据到字节跨度中
            var r = this.FileStorage.Read(this.Position, span);
            // 使用Interlocked类原子性地更新当前位置，以确保多线程环境下的安全性
            Interlocked.Add(ref this.m_position, r);
            // 返回实际读取到的字节数
            return r;
        }

        /// <inheritdoc/>
        protected override void SafetyDispose(bool disposing)
        {
            FilePool.TryReleaseFile(this.FileStorage.Path);
            this.FileStorage = null;
        }
    }
}