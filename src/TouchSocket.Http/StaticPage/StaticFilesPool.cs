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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// 静态文件缓存池
    /// </summary>
    public class StaticFilesPool : DisposableObject
    {
        private readonly Dictionary<string, StaticEntry> m_entriesByKey = new Dictionary<string, StaticEntry>();
        private readonly Dictionary<string, FolderEntry> m_foldersByKey = new Dictionary<string, FolderEntry>();
        private readonly ReaderWriterLockSlim m_lockSlim = new ReaderWriterLockSlim();
        private readonly SingleTimer m_timer;

        public StaticFilesPool()
        {
            this.m_timer = new SingleTimer(1000, () =>
            {
                var FolderEntry = m_foldersByKey.Values.Where(a => a.Changed);

                foreach (var item in FolderEntry)
                {
                    this.RemoveFolder(item.Path);
                    this.AddFolder(item.Path);
                }
            });
        }

        #region 属性

        /// <summary>
        /// 判断文件缓存是否为空
        /// </summary>
        public bool Empty => this.m_entriesByKey.Count == 0;

        /// <summary>
        /// 获取文件缓存的数量
        /// </summary>
        public int Count => this.m_entriesByKey.Count;

        /// <summary>
        /// 对于静态资源缓存的最大尺寸。默认1024*1024。
        /// </summary>
        /// <remarks>
        /// 大于设定值的资源只会存储<see cref="FileInfo"/>，每次访问资源时都会从磁盘加载。
        /// </remarks>
        public int MaxCacheSize { get; set; } = 1024*1024;
        #endregion 属性

        public void Clear()
        {
            using (new WriteLock(this.m_lockSlim))
            {
                foreach (var fileCacheEntry in this.m_foldersByKey)
                {
                    fileCacheEntry.Value.StopWatcher();
                    fileCacheEntry.Value.Clear();
                }

                this.m_entriesByKey.Clear();
                this.m_foldersByKey.Clear();
            }
        }

        #region Entry

        /// <summary>
        /// 添加一个新的缓存值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public bool AddEntry(string key, byte[] value, TimeSpan millisecondsTimeout)
        {
            using (new WriteLock(this.m_lockSlim))
            {
                key = FileUtility.PathFormat(key);
                this.m_entriesByKey.AddOrUpdate(key, new StaticEntry(value, millisecondsTimeout));
                return true;
            }
        }

        public bool AddEntry(string key, FileInfo value, TimeSpan millisecondsTimeout)
        {
            using (new WriteLock(this.m_lockSlim))
            {
                key = FileUtility.PathFormat(key);
                this.m_entriesByKey.AddOrUpdate(key, new StaticEntry(value, millisecondsTimeout));
                return true;
            }
        }

        public bool ContainsEntry(string key)
        {
            using (new ReadLock(this.m_lockSlim))
            {
                key = FileUtility.PathFormat(key);
                return this.m_entriesByKey.ContainsKey(key);
            }
        }

        public bool RemoveEntry(string key)
        {
            using (new WriteLock(this.m_lockSlim))
            {
                key = FileUtility.PathFormat(key);
                return this.m_entriesByKey.Remove(key);
            }
        }

        public bool TryFindEntry(string key, out StaticEntry cacheEntry)
        {
            using (new ReadLock(this.m_lockSlim))
            {
                key = FileUtility.PathFormat(key);
                return this.m_entriesByKey.TryGetValue(key, out cacheEntry);
            }
        }

        #endregion Entry

        #region Folder

        public int AddFolder(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = default)
        {
            ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(path, nameof(path));

            path = FileUtility.PathFormat(path);
            path = path.EndsWith("/") ? path : path + "/";

            timeout ??= TimeSpan.FromHours(1);

            this.RemoveFolder(path);

            var folderEntry = new FolderEntry(this, prefix, path, filter, timeout.Value);

            using (new WriteLock(this.m_lockSlim))
            {
                this.m_foldersByKey.Add(path, folderEntry);
            }

            var count = 0 ;
            this.PrivateAddFolder(folderEntry,ref count, path, path, prefix, timeout.Value);
            return count;
        }

        public bool ContainsFolder(string path)
        {
            using (new ReadLock(this.m_lockSlim))
            {
                path = FileUtility.PathFormat(path);
                path = path.EndsWith("/") ? path : path + "/";

                return this.m_foldersByKey.ContainsKey(path);
            }
        }

        public bool RemoveFolder(string path)
        {
            using (new WriteLock(this.m_lockSlim))
            {
                path = FileUtility.PathFormat(path);
                path = path.EndsWith("/") ? path : path + "/";

                if (!this.m_foldersByKey.TryGetValue(path, out var folderEntry))
                {
                    return false;
                }

                folderEntry.StopWatcher();

                foreach (var entryKey in folderEntry)
                {
                    this.m_entriesByKey.Remove(entryKey);
                }

                this.m_foldersByKey.Remove(path);

                return true;
            }
        }

        #endregion Folder

        internal bool InternalAddFile(string file, string key, TimeSpan millisecondsTimeout)
        {
            try
            {
                key = FileUtility.PathFormat(key);

                var fileInfo = new FileInfo(file);
                if (fileInfo.Length < this.MaxCacheSize)
                {
                    var content = File.ReadAllBytes(file);
                    if (!this.AddEntry(key, content, millisecondsTimeout))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!this.AddEntry(key, fileInfo, millisecondsTimeout))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal bool InternalRemoveFile(string path, string key)
        {
            try
            {
                key = FileUtility.PathFormat(key);
                return this.RemoveEntry(key);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (new WriteLock(this.m_lockSlim))
                {
                    this.Clear();
                }
            }

            base.Dispose(disposing);
        }

        private void PrivateAddFolder(FolderEntry folderEntry,ref int count, string root, string path, string prefix, TimeSpan timeout)
        {
            try
            {
                var keyPrefix = (string.IsNullOrEmpty(prefix) || (prefix == "/")) ? "/" : (prefix + "/");

                foreach (var item in Directory.GetDirectories(path))
                {
                    var key = keyPrefix + HttpUtility.UrlDecode(Path.GetFileName(item));

                    this.PrivateAddFolder(folderEntry, ref count, root, item, key, timeout);
                }

                foreach (var item in Directory.GetFiles(path))
                {
                    count++;

                    var key = keyPrefix + HttpUtility.UrlDecode(Path.GetFileName(item));

                    if (this.InternalAddFile(item, key, timeout))
                    {
                        using (new WriteLock(this.m_lockSlim))
                        {
                            folderEntry.Add(key);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}