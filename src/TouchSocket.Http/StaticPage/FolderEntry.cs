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

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Http
{
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
            this.m_watcher = new FileSystemWatcher();

            // Start the filesystem watcher
            this.StartWatcher(path, filter);
        }

        public void StopWatcher()
        {
            this.m_watcher.Dispose();
        }



        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            m_changed = true;
            return;
            //var fullPath = FileUtility.PathFormat(e.FullPath);

            //if (FileUtility.IsDirectory(fullPath))
            //{
            //    return;
            //}

            //var exists = SpinWait.SpinUntil(() => File.Exists(e.FullPath), Timeout);

            //if (exists)
            //{
            //    var key = FileUtility.PathFormat(e.FullPath).Replace(this.m_path, this.m_prefix);
            //    this.m_staticFileCachePool.InternalAddFile(fullPath, key, this.m_timespan);
            //}
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            m_changed = true;
            return;

            //var fullPath = FileUtility.PathFormat(e.FullPath);
            //if (FileUtility.IsDirectory(fullPath))
            //{
            //    return;
            //}

            //var key = FileUtility.PathFormat(fullPath).Replace(this.m_path, this.m_prefix);
            //this.m_staticFileCachePool.InternalRemoveFile(this.m_path, key);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            m_changed = true;
            return;

            //if (e.ChangeType != WatcherChangeTypes.Changed)
            //{
            //    return;
            //}

            //var fullPath = FileUtility.PathFormat(e.FullPath);

            //var key = fullPath.Replace(this.m_path, this.m_prefix);
            //var file = fullPath;
            //if (FileUtility.IsDirectory(file))
            //{
            //    return;
            //}

            //var exists = SpinWait.SpinUntil(() => File.Exists(file), Timeout);
            //if (exists)
            //{
            //    this.m_staticFileCachePool.InternalAddFile(file, key, this.m_timespan);
            //}
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            m_changed = true;
            return;

            //var oldKey = FileUtility.PathFormat(e.OldFullPath).Replace(this.m_path, this.m_prefix);
            //var oldFile = e.OldFullPath;
            //var newKey = FileUtility.PathFormat(e.FullPath).Replace(this.m_path, this.m_prefix);
            //var newFile = e.FullPath;
            //if (FileUtility.IsDirectory(newFile))
            //{
            //    return;
            //}

            //var exists = SpinWait.SpinUntil(() => File.Exists(newFile), Timeout);
            //if (exists)
            //{
            //    this.m_staticFileCachePool.InternalRemoveFile(this.m_path, oldKey);
            //    this.m_staticFileCachePool.InternalAddFile(newFile, newKey, this.m_timespan);
            //}
        }

        private void StartWatcher(string path, string filter)
        {
            this.m_watcher.Created += this.OnCreated;
            this.m_watcher.Deleted += this.OnDeleted;
            this.m_watcher.Renamed += this.OnRenamed;
            this.m_watcher.Changed += this.OnChanged;
            this.m_watcher.Path = path;
            this.m_watcher.IncludeSubdirectories = true;
            this.m_watcher.Filter = filter;
            this.m_watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
            this.m_watcher.EnableRaisingEvents = true;
        }
    };
}