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

namespace TouchSocket.Http;

internal class FolderEntry : HashSet<string>
{
    public const int Timeout = 1000 * 5;


    private readonly string m_path;
    private readonly string m_prefix;
    private readonly StaticFilesPool m_staticFileCachePool;
    private readonly TimeSpan m_timespan;
    private readonly FileSystemWatcher m_watcher;
    private volatile bool m_changed;

    public bool Changed => this.m_changed;

    public string Path => this.m_path;

    public FolderEntry(StaticFilesPool staticFileCachePool, string prefix, string path, string filter, TimeSpan timespan)
    {
        this.m_staticFileCachePool = staticFileCachePool;
        this.m_prefix = prefix;
        this.m_path = path;
        this.m_timespan = timespan;
        this.m_watcher = new FileSystemWatcher(path, filter);

        this.m_watcher.EnableRaisingEvents = true;
        this.m_watcher.Created += this.OnCreated;
        this.m_watcher.Deleted += this.OnDeleted;
        this.m_watcher.Renamed += this.OnRenamed;
        this.m_watcher.Changed += this.OnChanged;
        this.m_watcher.IncludeSubdirectories = true;
        this.m_watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
    }

    public void StopWatcher()
    {
        this.m_watcher.Dispose();
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        this.m_changed = true;
        return;
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        this.m_changed = true;
        return;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        this.m_changed = true;
        return;
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        this.m_changed = true;
        return;
    }
}