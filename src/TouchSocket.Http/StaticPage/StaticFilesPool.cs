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

using System.Web;

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
        this.m_timer = new SingleTimer(1000, this.ProcessChangedDirectories);
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
    /// 清除所有文件夹监控及缓存。
    /// </summary>
    public void Clear()
    {
        using (new WriteLock(this.m_lockSlim))
        {
            foreach (var entry in this.m_foldersByKey.Values)
            {
                entry.Dispose();
            }
            this.m_foldersByKey.Clear();
            this.m_entriesByKey.Clear();
        }
    }

    #region Entry

    /// <summary>
    /// 添加一个新的缓存值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值，以字节数组形式存储</param>
    /// <param name="millisecondsTimeout">缓存条目的超时时间</param>
    /// <returns>始终返回<see langword="true"/>，表示添加操作已完成</returns>
    public bool AddEntry(string key, byte[] value, TimeSpan millisecondsTimeout)
    {
        using (new WriteLock(this.m_lockSlim))
        {
            key = FileUtility.PathFormat(key);
            this.m_entriesByKey.AddOrUpdate(key, new StaticEntry(value, millisecondsTimeout));
            return true;
        }
    }

    /// <summary>
    /// 向缓存中添加一个条目。
    /// </summary>
    /// <param name="key">要添加的条目的键。</param>
    /// <param name="value">要添加的条目的值，包含文件信息。</param>
    /// <param name="millisecondsTimeout">条目过期的时间段。</param>
    /// <returns>总是返回<see langword="true"/>，表示条目已成功添加。</returns>
    public bool AddEntry(string key, FileInfo value, TimeSpan millisecondsTimeout)
    {
        using (new WriteLock(this.m_lockSlim))
        {
            key = FileUtility.PathFormat(key);
            this.m_entriesByKey.AddOrUpdate(key, new StaticEntry(value, millisecondsTimeout));
            return true;
        }
    }

    /// <summary>
    /// 检查给定键是否存在于条目中。
    /// </summary>
    /// <param name="key">要检查的键。</param>
    /// <returns>如果键存在于条目中，则返回 true；否则返回 false。</returns>
    public bool ContainsEntry(string key)
    {
        using (new ReadLock(this.m_lockSlim))
        {
            key = FileUtility.PathFormat(key);
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
        using (new WriteLock(this.m_lockSlim))
        {
            key = FileUtility.PathFormat(key);
            return this.m_entriesByKey.Remove(key);
        }
    }

    /// <summary>
    /// 尝试查找缓存项。若缓存未命中且存在受监控的文件夹，则尝试从磁盘按需加载。
    /// </summary>
    /// <param name="key">要查找的键。</param>
    /// <param name="cacheEntry">找到的缓存项，通过引用返回。</param>
    /// <returns>如果找到缓存项则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public bool TryFindEntry(string key, out StaticEntry cacheEntry)
    {
        string normalizedKey;
        using (new ReadLock(this.m_lockSlim))
        {
            normalizedKey = FileUtility.PathFormat(key);
            if (!normalizedKey.StartsWith("/"))
            {
                normalizedKey = "/" + normalizedKey;
            }
            if (this.m_entriesByKey.TryGetValue(normalizedKey, out cacheEntry))
            {
                return true;
            }
        }

        return this.TryLoadOnDemand(normalizedKey, out cacheEntry);
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
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(path, nameof(path));

        path = FileUtility.PathFormat(path);
        path = path.EndsWith("/") ? path : path + "/";
        timeout ??= TimeSpan.FromHours(1);

        this.RemoveFolder(path);

        var folderEntry = new FolderEntry(path, prefix, filter, timeout.Value);

        using (new WriteLock(this.m_lockSlim))
        {
            this.m_foldersByKey.Add(path, folderEntry);
        }

        var count = 0;
        this.PrivateLoadDirectory(folderEntry, ref count, path, prefix, timeout.Value);
        return count;
    }

    /// <summary>
    /// 检查指定路径的文件夹是否存在于集合中。
    /// </summary>
    /// <param name="path">要检查的文件夹路径。</param>
    /// <returns>如果文件夹存在于集合中，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public bool ContainsFolder(string path)
    {
        using (new ReadLock(this.m_lockSlim))
        {
            path = FileUtility.PathFormat(path);
            path = path.EndsWith("/") ? path : path + "/";
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
        using (new WriteLock(this.m_lockSlim))
        {
            path = FileUtility.PathFormat(path);
            path = path.EndsWith("/") ? path : path + "/";

            if (!this.m_foldersByKey.TryGetValue(path, out var folderEntry))
            {
                return false;
            }

            folderEntry.Dispose();

            foreach (var key in folderEntry.GetAllUrlKeys())
            {
                this.m_entriesByKey.Remove(key);
            }

            this.m_foldersByKey.Remove(path);
            return true;
        }
    }

    #endregion Folder

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
            this.m_timer.Dispose();
            this.Clear();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// 计算物理目录对应的 URL 前缀。
    /// </summary>
    private static string ComputeUrlPrefix(string rootPath, string rootPrefix, string normalizedPhysicalDir)
    {
        var rel = normalizedPhysicalDir.Length > rootPath.Length
            ? normalizedPhysicalDir.Substring(rootPath.Length).Trim('/')
            : string.Empty;

        var basePrefix = string.IsNullOrEmpty(rootPrefix) || rootPrefix == "/"
            ? string.Empty
            : rootPrefix.TrimEnd('/');

        return string.IsNullOrEmpty(rel)
            ? (string.IsNullOrEmpty(basePrefix) ? "/" : basePrefix)
            : basePrefix + "/" + rel;
    }

    /// <summary>
    /// 对待重载目录列表去重：若父目录已在列表中，则跳过其子目录。
    /// </summary>
    private static List<string> DeduplicateParentDirs(List<string> dirs)
    {
        if (dirs.Count <= 1)
        {
            return dirs;
        }

        dirs.Sort((a, b) => a.Length.CompareTo(b.Length));
        var result = new List<string>();
        foreach (var dir in dirs)
        {
            if (!result.Exists(r => dir.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(dir);
            }
        }
        return result;
    }

    /// <summary>
    /// 定时器回调：扫描所有有变更的文件夹，按子目录粒度精准重载。
    /// </summary>
    private void ProcessChangedDirectories()
    {
        List<(FolderEntry folder, List<string> changedDirs)> workItems;

        using (new ReadLock(this.m_lockSlim))
        {
            workItems = this.m_foldersByKey.Values
                .Where(f => f.HasChanges)
                .Select(f => (f, f.DequeueChangedDirs()))
                .Where(x => x.Item2.Count > 0)
                .ToList();
        }

        foreach (var (folder, changedDirs) in workItems)
        {
            var deduped = DeduplicateParentDirs(changedDirs);
            foreach (var dir in deduped)
            {
                this.ReloadDirectory(folder, dir);
            }
        }
    }

    /// <summary>
    /// 重载指定物理目录（含子目录树）的缓存。
    /// </summary>
    private void ReloadDirectory(FolderEntry folder, string normalizedPhysicalDir)
    {
        using (new WriteLock(this.m_lockSlim))
        {
            if (!this.m_foldersByKey.ContainsValue(folder))
            {
                return;
            }

            var removedKeys = folder.RemoveDirectoryTree(normalizedPhysicalDir);
            foreach (var key in removedKeys)
            {
                this.m_entriesByKey.Remove(key);
            }
        }

        var physicalPath = normalizedPhysicalDir.TrimEnd('/');
        if (!Directory.Exists(physicalPath))
        {
            return;
        }

        var urlPrefix = ComputeUrlPrefix(folder.RootPath, folder.Prefix, normalizedPhysicalDir);
        var count = 0;
        this.PrivateLoadDirectory(folder, ref count, normalizedPhysicalDir, urlPrefix, folder.Timespan);
    }

    /// <summary>
    /// 递归加载物理目录下的所有文件至缓存，并向 <see cref="FolderEntry"/> 注册目录跟踪信息。
    /// </summary>
    private void PrivateLoadDirectory(FolderEntry folderEntry, ref int count, string normalizedPhysicalDir, string urlPrefix, TimeSpan timeout)
    {
        try
        {
            var physicalPath = normalizedPhysicalDir.TrimEnd('/');
            var keyPrefix = string.IsNullOrEmpty(urlPrefix) || urlPrefix == "/"
                ? "/"
                : urlPrefix.TrimEnd('/') + "/";

            // 在锁外读取文件内容，避免IO操作持锁
            var fileEntries = new List<(string key, StaticEntry entry)>();
            foreach (var file in Directory.GetFiles(physicalPath))
            {
                count++;
                var fileName = HttpUtility.UrlDecode(Path.GetFileName(file));
                var key = FileUtility.PathFormat(keyPrefix + fileName);
                if (this.TryCreateEntry(file, timeout, out var entry))
                {
                    fileEntries.Add((key, entry));
                }
            }

            // 批量写入缓存，减少锁竞争
            using (new WriteLock(this.m_lockSlim))
            {
                var keys = new List<string>(fileEntries.Count);
                foreach (var (key, entry) in fileEntries)
                {
                    this.m_entriesByKey.AddOrUpdate(key, entry);
                    keys.Add(key);
                }
                folderEntry.TrackDirectory(normalizedPhysicalDir, keys);
            }

            // 递归处理子目录
            foreach (var subDir in Directory.GetDirectories(physicalPath))
            {
                var subDirName = HttpUtility.UrlDecode(Path.GetFileName(subDir));
                var subPrefix = keyPrefix + subDirName;
                var normalizedSubDir = FileUtility.PathFormat(subDir) + "/";
                this.PrivateLoadDirectory(folderEntry, ref count, normalizedSubDir, subPrefix, timeout);
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 缓存未命中时，尝试从受监控的文件夹中按需加载文件至缓存。
    /// </summary>
    private bool TryLoadOnDemand(string normalizedKey, out StaticEntry cacheEntry)
    {
        cacheEntry = null;

        List<FolderEntry> folders;
        using (new ReadLock(this.m_lockSlim))
        {
            if (this.m_foldersByKey.Count == 0)
            {
                return false;
            }
            folders = this.m_foldersByKey.Values.ToList();
        }

        foreach (var folder in folders)
        {
            if (!TryResolvePhysicalPath(folder, normalizedKey, out var physicalPath))
            {
                continue;
            }

            if (!File.Exists(physicalPath))
            {
                continue;
            }

            if (!FileMatchesFilter(Path.GetFileName(physicalPath), folder.Filter))
            {
                continue;
            }

            if (!this.TryCreateEntry(physicalPath, folder.Timespan, out var entry))
            {
                continue;
            }

            var dirFullPath = Path.GetDirectoryName(physicalPath);
            if (dirFullPath == null)
            {
                continue;
            }
            var dirPath = FileUtility.PathFormat(dirFullPath) + "/";

            using (new WriteLock(this.m_lockSlim))
            {
                this.m_entriesByKey.AddOrUpdate(normalizedKey, entry);
                folder.AddKeyToDirectory(dirPath, normalizedKey);
            }

            cacheEntry = entry;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 根据 URL 键和文件夹配置，反向计算对应的物理文件路径。
    /// </summary>
    private static bool TryResolvePhysicalPath(FolderEntry folder, string normalizedKey, out string physicalPath)
    {
        physicalPath = null;

        var prefix = string.IsNullOrEmpty(folder.Prefix) || folder.Prefix == "/"
            ? string.Empty
            : folder.Prefix.TrimEnd('/');

        string relativePart;
        if (string.IsNullOrEmpty(prefix))
        {
            if (!normalizedKey.StartsWith("/", StringComparison.Ordinal))
            {
                return false;
            }
            relativePart = normalizedKey.Substring(1);
        }
        else
        {
            if (!normalizedKey.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            relativePart = normalizedKey.Substring(prefix.Length + 1);
        }

        if (string.IsNullOrEmpty(relativePart))
        {
            return false;
        }

        var relativeOsPath = relativePart.Replace('/', Path.DirectorySeparatorChar);
        physicalPath = Path.Combine(folder.RootPath.TrimEnd('/'), relativeOsPath);

        // 路径安全校验：防止路径遍历攻击
        var rootFull = Path.GetFullPath(folder.RootPath.TrimEnd('/'));
        var fileFull = Path.GetFullPath(physicalPath);
        if (!fileFull.StartsWith(rootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            physicalPath = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 判断文件名是否匹配指定过滤器（支持 <c>*.*</c>、<c>*.ext</c> 等简单通配符）。
    /// </summary>
    private static bool FileMatchesFilter(string fileName, string filter)
    {
        if (filter is "*.*" or "*")
        {
            return true;
        }
        if (filter.StartsWith("*.", StringComparison.Ordinal))
        {
            return fileName.EndsWith(filter.Substring(1), StringComparison.OrdinalIgnoreCase);
        }
        return string.Equals(fileName, filter, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 尝试根据文件路径创建 <see cref="StaticEntry"/>。
    /// </summary>
    private bool TryCreateEntry(string file, TimeSpan timeout, out StaticEntry entry)
    {
        try
        {
            var fileInfo = new FileInfo(file);
            entry = fileInfo.Length < this.MaxCacheSize
                ? new StaticEntry(File.ReadAllBytes(file), timeout)
                : new StaticEntry(fileInfo, timeout);
            return true;
        }
        catch
        {
            entry = null;
            return false;
        }
    }
}