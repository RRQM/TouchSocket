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
using System.IO;
using System.Threading;
using TouchSocket.Resources;

namespace TouchSocket.Core.IO
{
    /// <summary>
    /// 文件存储器。
    /// </summary>
    public class FileStorage : IDisposable
    {
        internal int m_reference;
        private bool m_cache;
        private bool m_disposedValue;
        private FileAccess? m_fileAccess;
        private byte[] m_fileData;
        private FileInfo m_fileInfo;
        private FileStream m_fileStream;
        private readonly ReaderWriterLockSlim m_lockSlim;
        private string m_path;

        private FileStorage()
        {
            this.m_lockSlim = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~FileStorage()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// 写入时清空缓存区
        /// </summary>
        public void Flush()
        {
            this.m_fileStream.Flush();
        }

        /// <summary>
        /// 访问属性
        /// </summary>
        public FileAccess? Access => this.m_fileAccess;

        /// <summary>
        /// 是否为缓存型。为false时，意味着该文件句柄正在被该程序占用。
        /// </summary>
        public bool Cache => this.m_cache;

        /// <summary>
        /// 文件信息
        /// </summary>
        public FileInfo FileInfo => this.m_fileInfo;

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length => this.m_fileStream.Length;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path => this.m_path;

        /// <summary>
        /// 引用次数。
        /// </summary>
        public int Reference => this.m_reference;

        /// <summary>
        /// 文件流。
        /// 一般情况下，请不要直接访问该对象。否则有可能会产生不可预测的错误。
        /// </summary>
        public FileStream FileStream { get => this.m_fileStream; }

        /// <summary>
        /// 创建一个只读的、已经缓存的文件信息。该操作不会占用文件句柄。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileStorage"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool TryCreateCacheFileStorage(string path, out FileStorage fileStorage, out string msg)
        {
            path = System.IO.Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                fileStorage = null;
                msg = TouchSocketRes.FileNotExists.GetDescription(path);
                return false;
            }
            try
            {
                fileStorage = new FileStorage()
                {
                    m_cache = true,
                    m_fileAccess = FileAccess.Read,
                    m_fileInfo = new FileInfo(path),
                    m_path = path,
                    m_reference = 0,
                    m_fileData = File.ReadAllBytes(path)
                };
                msg = null;
                return true;
            }
            catch (Exception ex)
            {
                fileStorage = null;
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 创建一个文件流句柄
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileAccess"></param>
        /// <param name="fileStorage"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool TryCreateFileStorage(string path, FileAccess fileAccess, out FileStorage fileStorage, out string msg)
        {
            path = System.IO.Path.GetFullPath(path);
            if (fileAccess == FileAccess.Read && !File.Exists(path))
            {
                fileStorage = null;
                msg = TouchSocketRes.FileNotExists.GetDescription(path);
                return false;
            }
            try
            {
                fileStorage = new FileStorage()
                {
                    m_fileAccess = fileAccess,
                    m_fileInfo = new FileInfo(path),
                    m_path = path,
                    m_reference = 0,
                    m_fileStream = new FileStream(path, FileMode.OpenOrCreate, fileAccess)
                };
                msg = null;
                return true;
            }
            catch (Exception ex)
            {
                fileStorage = null;
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.m_reference > 0)
            {
                throw new Exception("当前对象引用次数不为0，请先降低其引用。");
            }
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 从指定位置，读取数据到缓存区。线程安全。
        /// </summary>
        /// <param name="stratPos"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(long stratPos, byte[] buffer, int offset, int length)
        {
            using (WriteLock writeLock = new WriteLock(this.m_lockSlim))
            {
                if (this.m_disposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.m_fileAccess != FileAccess.Read)
                {
                    throw new Exception("该流不允许读取。");
                }
                if (this.m_cache)
                {
                    int r = (int)Math.Min(this.m_fileData.Length - stratPos, length);
                    Array.Copy(this.m_fileData, stratPos, buffer, offset, r);
                    return r;
                }
                else
                {
                    this.m_fileStream.Position = stratPos;
                    return this.m_fileStream.Read(buffer, offset, length);
                }
            }
        }

        /// <summary>
        /// 从指定位置，写入数据到存储区。线程安全。
        /// </summary>
        /// <param name="stratPos"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Write(long stratPos, byte[] buffer, int offset, int length)
        {
            using (WriteLock writeLock = new WriteLock(this.m_lockSlim))
            {
                if (this.m_disposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.m_fileAccess != FileAccess.Write)
                {
                    throw new Exception("该流不允许写入。");
                }
                this.m_fileStream.Position = stratPos;
                this.m_fileStream.Write(buffer, offset, length);
                this.m_fileStream.Flush();
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            using (WriteLock writeLock = new WriteLock(this.m_lockSlim))
            {
                if (!this.m_disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)
                    }
                    this.m_fileStream.SafeDispose();
                    this.m_fileData = null;
                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    this.m_disposedValue = true;
                }
            }
        }
    }
}