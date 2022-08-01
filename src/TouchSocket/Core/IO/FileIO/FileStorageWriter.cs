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

namespace TouchSocket.Core.IO
{
    /// <summary>
    /// 文件写入器。
    /// </summary>
    public class FileStorageWriter : DisposableObject
    {
        private readonly FileStorage m_fileStorage;
        private readonly bool m_singleRef;
        private long m_position;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileStorage"></param>
        /// <param name="singleRef"></param>
        public FileStorageWriter(FileStorage fileStorage, bool singleRef)
        {
            this.m_fileStorage = fileStorage;
            this.m_singleRef = singleRef;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~FileStorageWriter()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: false);
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

        /// <summary>
        /// 文件存储器
        /// </summary>
        public FileStorage FileStorage => this.m_fileStorage;

        /// <summary>
        /// 游标位置
        /// </summary>
        public int Pos
        {
            get => (int)this.m_position;
            set => this.m_position = value;
        }

        /// <summary>
        /// 游标位置
        /// </summary>
        public long Position
        {
            get => this.m_position;
            set => this.m_position = value;
        }


        /// <summary>
        /// 读取数据到缓存区
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public void Write(byte[] buffer, int offset, int length)
        {
            this.m_fileStorage.Write(this.m_position, buffer, offset, length);
            this.m_position += length;
        }


    }
}