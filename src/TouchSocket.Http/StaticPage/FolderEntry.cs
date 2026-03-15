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

using System.Collections.Concurrent;

namespace TouchSocket.Http;

/// <summary>
/// 表示一个受监控的文件夹条目，支持按子目录粒度跟踪文件系统变更。
/// </summary>
internal sealed class FolderEntry : IDisposable
{
    // 物理目录路径（已规范化，使用'/'并带尾部'/'） → 该目录下的 URL 键集合（非递归）
    // 仅在 StaticFilesPool 的写锁下访问
    private readonly Dictionary<string, HashSet<string>> m_dirToKeys = new(StringComparer.OrdinalIgnoreCase);

    // 已知的物理目录路径集合，供事件处理器无锁读取
    private readonly ConcurrentDictionary<string, byte> m_knownDirs = new(StringComparer.OrdinalIgnoreCase);

    // 待重载的物理目录路径队列（线程安全）
    private readonly ConcurrentDictionary<string, byte> m_changedDirs = new(StringComparer.OrdinalIgnoreCase);

    private readonly FileSystemWatcher m_watcher;
    private bool m_disposed;

    /// <summary>
    /// 初始化 <see cref="FolderEntry"/>。
    /// </summary>
    /// <param name="rootPath">根物理路径（已规范化，带尾部'/'）。</param>
    /// <param name="prefix">URL 前缀。</param>
    /// <param name="filter">文件过滤器。</param>
    /// <param name="timespan">缓存超时时长。</param>
    public FolderEntry(string rootPath, string prefix, string filter, TimeSpan timespan)
    {
        this.RootPath = rootPath;
        this.Prefix = prefix;
        this.Filter = filter;
        this.Timespan = timespan;

        this.m_watcher = new FileSystemWatcher(rootPath.TrimEnd('/'), filter)
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
        };
        this.m_watcher.Created += this.OnCreated;
        this.m_watcher.Deleted += this.OnDeleted;
        this.m_watcher.Changed += this.OnChanged;
        this.m_watcher.Renamed += this.OnRenamed;
    }

    /// <summary>根物理路径（带尾部'/'，使用'/'分隔符）。</summary>
    public string RootPath { get; }

    /// <summary>URL 前缀。</summary>
    public string Prefix { get; }

    /// <summary>文件过滤器。</summary>
    public string Filter { get; }

    /// <summary>缓存超时时长。</summary>
    public TimeSpan Timespan { get; }

    /// <summary>是否存在待处理的变更目录。</summary>
    public bool HasChanges => !this.m_changedDirs.IsEmpty;

    /// <summary>
    /// 注册某物理目录及其对应的 URL 键列表。在 <see cref="StaticFilesPool"/> 写锁下调用。
    /// </summary>
    public void TrackDirectory(string normalizedDirPath, IReadOnlyList<string> urlKeys)
    {
        this.m_dirToKeys[normalizedDirPath] = new HashSet<string>(urlKeys);
        this.m_knownDirs.TryAdd(normalizedDirPath, 0);
    }

    /// <summary>
    /// 返回所有已跟踪目录下的全部 URL 键。在 <see cref="StaticFilesPool"/> 读/写锁下调用。
    /// </summary>
    public IEnumerable<string> GetAllUrlKeys()
    {
        foreach (var set in this.m_dirToKeys.Values)
        {
            foreach (var key in set)
            {
                yield return key;
            }
        }
    }

    /// <summary>
    /// 移除指定物理目录及其所有子目录的跟踪信息，返回被移除的 URL 键列表。在写锁下调用。
    /// </summary>
    public List<string> RemoveDirectoryTree(string normalizedDirPath)
    {
        var result = new List<string>();
        foreach (var kvp in this.m_dirToKeys.ToList())
        {
            if (IsSubpathOrSame(kvp.Key, normalizedDirPath))
            {
                result.AddRange(kvp.Value);
                this.m_dirToKeys.Remove(kvp.Key);
                this.m_knownDirs.TryRemove(kvp.Key, out _);
            }
        }
        return result;
    }

    /// <summary>
    /// 取出并清除所有待重载的目录列表。
    /// </summary>
    public List<string> DequeueChangedDirs()
    {
        var dirs = this.m_changedDirs.Keys.ToList();
        foreach (var d in dirs)
        {
            this.m_changedDirs.TryRemove(d, out _);
        }
        return dirs;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.m_disposed)
        {
            this.m_disposed = true;
            this.m_watcher.Dispose();
        }
    }

    private static string NormalizeDirPath(string path)
    {
        var s = path.Replace('\\', '/');
        return s.EndsWith("/") ? s : s + '/';
    }

    private static bool IsSubpathOrSame(string candidate, string parentWithTrailingSlash)
    {
        return candidate.Equals(parentWithTrailingSlash, StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith(parentWithTrailingSlash, StringComparison.OrdinalIgnoreCase);
    }

    private void MarkDirChanged(string physicalPath)
    {
        this.m_changedDirs.TryAdd(NormalizeDirPath(physicalPath), 0);
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            // 新建的若为目录则标记该目录，否则标记其父目录
            if (Directory.Exists(e.FullPath))
                this.MarkDirChanged(e.FullPath);
            else
                this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.FullPath) ?? this.RootPath);
        }
        catch
        {
            this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.FullPath) ?? this.RootPath);
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (Directory.Exists(e.FullPath))
                this.MarkDirChanged(e.FullPath);
            else
                this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.FullPath) ?? this.RootPath);
        }
        catch
        {
            this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.FullPath) ?? this.RootPath);
        }
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        // 被删除的路径已不存在，通过 m_knownDirs 判断它是否为目录
        var normalized = NormalizeDirPath(e.FullPath);
        if (this.m_knownDirs.ContainsKey(normalized))
            this.m_changedDirs.TryAdd(normalized, 0);
        else
            this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.FullPath) ?? this.RootPath);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        // 标记旧路径
        var oldNorm = NormalizeDirPath(e.OldFullPath);
        if (this.m_knownDirs.ContainsKey(oldNorm))
            this.m_changedDirs.TryAdd(oldNorm, 0);
        else
            this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.OldFullPath) ?? this.RootPath);

        // 标记新路径
        try
        {
            if (Directory.Exists(e.FullPath))
                this.MarkDirChanged(e.FullPath);
            else
                this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.FullPath) ?? this.RootPath);
        }
        catch
        {
            this.MarkDirChanged(System.IO.Path.GetDirectoryName(e.FullPath) ?? this.RootPath);
        }
    }
}