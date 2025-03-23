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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// 静态文件缓存池
/// </summary>
public class StaticFilesPool : DisposableObject
{
    private readonly Dictionary<string, StaticEntry> m_entriesByKey = new Dictionary<string, StaticEntry>();
    private readonly Dictionary<string, FolderEntry> m_foldersByKey = new Dictionary<string, FolderEntry>();
    private readonly ReaderWriterLockSlim m_lockSlim = new ReaderWriterLockSlim();
    private readonly SingleTimer m_timer;

    /// <summary>
    /// 静态文件池的构造函数
    /// </summary>
    public StaticFilesPool()
    {
        this.m_timer = new SingleTimer(1000, () =>
        {
            var folderEntry = this.m_foldersByKey.Values.Where(a => a.Changed);

            foreach (var item in folderEntry)
            {
                this.RemoveFolder(item.Path);
                this.AddFolder(item.Path, item.Prefix,item.Filter,item.Timespan);
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
    public int MaxCacheSize { get; set; } = 1024 * 1024;
    #endregion 属性

    /// <summary>
    /// 清除缓存。
    /// 该方法通过停止文件监视器并清除缓存项，来释放资源。
    /// </summary>
    public void Clear()
    {
        // 使用写锁保护访问共享资源，防止并发访问时的一致性问题。
        using (new WriteLock(this.m_lockSlim))
        {
            // 遍历所有缓存项，停止文件系统监视器并清除缓存内容。
            foreach (var fileCacheEntry in this.m_foldersByKey)
            {
                fileCacheEntry.Value.StopWatcher();
                fileCacheEntry.Value.Clear();
            }

            // 清空缓存项映射，确保没有悬挂的引用。
            this.m_entriesByKey.Clear();
            this.m_foldersByKey.Clear();
        }
    }

    #region Entry

    /// <summary>
    /// 添加一个新的缓存值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值，以字节数组形式存储</param>
    /// <param name="millisecondsTimeout">缓存条目的超时时间，以毫秒为单位</param>
    /// <returns>始终返回true，表示添加操作已完成</returns>
    public bool AddEntry(string key, byte[] value, TimeSpan millisecondsTimeout)
    {
        // 使用WriteLock确保在添加缓存条目时数据的一致性
        using (new WriteLock(this.m_lockSlim))
        {
            // 格式化键以保持一致性
            key = FileUtility.PathFormat(key);
            // 添加或更新缓存条目
            this.m_entriesByKey.AddOrUpdate(key, new StaticEntry(value, millisecondsTimeout));
            return true;
        }
    }

    /// <summary>
    /// 向缓存中添加一个条目。
    /// </summary>
    /// <param name="key">要添加的条目的键。</param>
    /// <param name="value">要添加的条目的值，包含文件信息。</param>
    /// <param name="millisecondsTimeout">条目过期的时间段，以毫秒为单位。</param>
    /// <returns>总是返回true，表示条目已成功添加。</returns>
    public bool AddEntry(string key, FileInfo value, TimeSpan millisecondsTimeout)
    {
        // 使用WriteLock确保在添加条目时数据的一致性
        using (new WriteLock(this.m_lockSlim))
        {
            // 格式化键值，确保路径格式正确
            key = FileUtility.PathFormat(key);
            // 添加或更新条目
            this.m_entriesByKey.AddOrUpdate(key, new StaticEntry(value, millisecondsTimeout));
            return true;
        }
    }

    /// <summary>
    /// 检查给定键是否存在于条目中。
    /// </summary>
    /// <param name="key">要检查的键。</param>
    /// <returns>如果键存在于条目中，则返回 true；否则返回 false。</returns>
    /// <remarks>
    /// 此方法用于确定是否已经存在具有给定键的条目。
    /// 它首先对键应用路径格式化，然后检查是否存在与格式化后的键相对应的条目。
    /// 使用 ReadLock 确保在读取操作期间数据的一致性。
    /// </remarks>
    public bool ContainsEntry(string key)
    {
        // 使用 ReadLock 以确保读取操作期间数据的一致性
        using (new ReadLock(this.m_lockSlim))
        {
            // 对键应用路径格式化以保持一致性
            key = FileUtility.PathFormat(key);
            // 检查键是否存在
            return this.m_entriesByKey.ContainsKey(key);
        }
    }
    /// <summary>
    /// 移除指定键的条目。
    /// </summary>
    /// <param name="key">要移除的条目的键。</param>
    /// <returns>如果成功移除，则返回 true；否则返回 false。</returns>
    public bool RemoveEntry(string key)
    {
        // 使用写锁保护对共享资源的访问
        using (new WriteLock(this.m_lockSlim))
        {
            // 格式化键值以确保一致性
            key = FileUtility.PathFormat(key);
            // 从键集合中移除指定的键
            return this.m_entriesByKey.Remove(key);
        }
    }

    /// <summary>
    /// 尝试查找缓存项。
    /// </summary>
    /// <param name="key">要查找的键。</param>
    /// <param name="cacheEntry">找到的缓存项，通过引用返回。</param>
    /// <returns>如果找到缓存项则返回true；否则返回false。</returns>
    public bool TryFindEntry(string key, out StaticEntry cacheEntry)
    {
        // 使用读锁来确保并发访问时的一致性
        using (new ReadLock(this.m_lockSlim))
        {
            // 格式化键，以确保键的一致性和正确性
            key = FileUtility.PathFormat(key);
            // 尝试根据键查找缓存项
            return this.m_entriesByKey.TryGetValue(key, out cacheEntry);
        }
    }

    #endregion Entry

    #region Folder

    /// <summary>
    /// 添加一个文件夹到监控列表。
    /// </summary>
    /// <param name="path">要添加的文件夹路径。</param>
    /// <param name="prefix">文件夹的前缀，默认为"/"。</param>
    /// <param name="filter">文件过滤器，默认为"*.*"，表示不过滤。</param>
    /// <param name="timeout">操作超时时间，默认为1小时。</param>
    /// <returns>添加的文件数量。</returns>
    public int AddFolder(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = default)
    {
        // 检查路径参数是否为空或无效
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(path, nameof(path));

        // 格式化路径，确保路径格式正确
        path = FileUtility.PathFormat(path);
        // 如果路径不以"/"结尾，添加之
        path = path.EndsWith("/") ? path : path + "/";

        // 设置默认超时时间为1小时
        timeout ??= TimeSpan.FromHours(1);

        // 移除已存在的同名文件夹，确保不重复添加
        this.RemoveFolder(path);

        // 创建一个新的文件夹条目
        var folderEntry = new FolderEntry(this, prefix, path, filter, timeout.Value);

        // 使用写锁保护文件夹集合，确保线程安全
        using (new WriteLock(this.m_lockSlim))
        {
            // 将新的文件夹条目添加到字典中
            this.m_foldersByKey.Add(path, folderEntry);
        }

        // 初始化计数器，用于记录添加的文件数量
        var count = 0;
        // 私有方法，实际执行添加文件夹操作
        this.PrivateAddFolder(folderEntry, ref count, path, path, prefix, timeout.Value);
        // 返回添加的文件数量
        return count;
    }

    /// <summary>
    /// 检查指定路径的文件夹是否存在于集合中。
    /// </summary>
    /// <param name="path">要检查的文件夹路径。</param>
    /// <returns>如果文件夹存在于集合中，则返回true；否则返回false。</returns>
    public bool ContainsFolder(string path)
    {
        // 使用读取锁确保线程安全
        using (new ReadLock(this.m_lockSlim))
        {
            // 格式化路径，确保一致性
            path = FileUtility.PathFormat(path);
            // 确保路径以斜杠结尾，以便于后续的比较
            path = path.EndsWith("/") ? path : path + "/";

            // 检查格式化后的路径是否存在于集合中
            return this.m_foldersByKey.ContainsKey(path);
        }
    }

    /// <summary>
    /// 移除指定路径的文件夹。
    /// </summary>
    /// <param name="path">要移除的文件夹路径。</param>
    /// <returns>如果成功移除文件夹，则返回 true；如果指定路径不存在于文件夹集合中，则返回 false。</returns>
    public bool RemoveFolder(string path)
    {
        // 使用写锁来确保线程安全
        using (new WriteLock(this.m_lockSlim))
        {
            // 格式化路径，确保一致性
            path = FileUtility.PathFormat(path);
            // 确保路径以斜杠结尾，以便于后续处理
            path = path.EndsWith("/") ? path : path + "/";

            // 尝试从文件夹字典中获取指定路径的文件夹条目
            if (!this.m_foldersByKey.TryGetValue(path, out var folderEntry))
            {
                // 如果找不到指定路径的文件夹条目，则返回 false
                return false;
            }

            // 停止文件夹条目的监视器，以防止进一步的资源监视
            folderEntry.StopWatcher();

            // 遍历文件夹条目中的所有子项，并从子项字典中移除
            foreach (var entryKey in folderEntry)
            {
                this.m_entriesByKey.Remove(entryKey);
            }

            // 从文件夹字典中移除该文件夹条目
            this.m_foldersByKey.Remove(path);

            // 成功移除文件夹条目，返回 true
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

    private void PrivateAddFolder(FolderEntry folderEntry, ref int count, string root, string path, string prefix, TimeSpan timeout)
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

                if (!this.InternalAddFile(item, key, timeout))
                {
                    continue;
                }

                using (new WriteLock(this.m_lockSlim))
                {
                    folderEntry.Add(key);
                }
            }
        }
        catch
        {
            // ignored
        }
    }
}