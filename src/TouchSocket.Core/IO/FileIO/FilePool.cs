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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 文件池。
    /// </summary>

    public static partial class FilePool
    {
        private static readonly object s_locker = new object();

        private static readonly ConcurrentDictionary<string, FileStorage> m_pathStorage = new ConcurrentDictionary<string, FileStorage>();

        private static readonly Timer s_timer;

        static FilePool()
        {
            s_timer = new Timer(OnTimer, null, 1000 * 60, 1000 * 60);
        }

        /// <summary>
        /// 获取所有的路径。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAllPaths()
        {
            return m_pathStorage.Keys;
        }

        private static void ThrowIfPathIsNull(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ThrowHelper.ThrowArgumentNullException(nameof(path));
            }
        }

        /// <summary>
        /// 加载文件为读取流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorage GetFileStorageForRead(string path)
        {
            ThrowIfPathIsNull(path);
            return GetFileStorageForRead(new FileInfo(path));
        }

        /// <summary>
        /// 加载文件为读取流
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorage GetFileStorageForRead(FileInfo fileInfo)
        {
            if (m_pathStorage.TryGetValue(fileInfo.FullName, out var storage))
            {
                if (storage.FileAccess != FileAccess.Read)
                {
                    ThrowHelper.ThrowException(TouchSocketCoreResource.FileOnlyWrittenTo.Format(fileInfo.FullName));
                }
                Interlocked.Increment(ref storage.m_reference);
                return storage;
            }
            lock (s_locker)
            {
                storage = new FileStorage(fileInfo, FileAccess.Read);
                if (m_pathStorage.TryAdd(fileInfo.FullName, storage))
                {
                    Interlocked.Increment(ref storage.m_reference);
                    return storage;
                }
                return GetFileStorageForRead(fileInfo);
            }
        }

        /// <summary>
        /// 加载文件为写流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorage GetFileStorageForWrite(string path)
        {
            ThrowIfPathIsNull(path);
            return GetFileStorageForWrite(new FileInfo(path));
        }

        /// <summary>
        /// 加载文件为写流
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorage GetFileStorageForWrite(FileInfo fileInfo)
        {
            if (m_pathStorage.TryGetValue(fileInfo.FullName, out var storage))
            {
                if (storage.FileAccess != FileAccess.Write)
                {
                    ThrowHelper.ThrowException(TouchSocketCoreResource.FileReadOnly.Format(fileInfo.FullName));
                }
                Interlocked.Increment(ref storage.m_reference);
                return storage;
            }
            lock (s_locker)
            {
                storage = new FileStorage(fileInfo, FileAccess.Write);
                if (m_pathStorage.TryAdd(fileInfo.FullName, storage))
                {
                    Interlocked.Increment(ref storage.m_reference);
                    return storage;
                }
                return GetFileStorageForWrite(fileInfo);
            }
        }

        /// <summary>
        /// 获取一个可读可写的Stream对象。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorageStream GetFileStorageStream(string path)
        {
            return new FileStorageStream(GetFileStorageForWrite(path));
        }

        /// <summary>
        /// 获取一个可读可写的Stream对象。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorageStream GetFileStorageStream(FileInfo fileInfo)
        {
            return new FileStorageStream(GetFileStorageForWrite(fileInfo));
        }

        /// <summary>
        /// 获取一个文件读取访问器
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorageReader GetReader(string path)
        {
            return new FileStorageReader(GetFileStorageForRead(path));
        }

        /// <summary>
        /// 获取一个文件读取访问器
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorageReader GetReader(FileInfo fileInfo)
        {
            return new FileStorageReader(GetFileStorageForRead(fileInfo));
        }

        /// <summary>
        /// 获取引用次数。
        /// </summary>
        /// <param name="path">必须是全路径。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static int GetReferenceCount(string path)
        {
            ThrowIfPathIsNull(path);
            return m_pathStorage.TryGetValue(path, out var fileStorage) ? fileStorage.m_reference : 0;
        }

        /// <summary>
        /// 获取一个文件写入访问器
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorageWriter GetWriter(string path)
        {
            return new FileStorageWriter(GetFileStorageForWrite(path));
        }

        /// <summary>
        /// 获取一个文件写入访问器
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static FileStorageWriter GetWriter(FileInfo fileInfo)
        {
            return new FileStorageWriter(GetFileStorageForWrite(fileInfo));
        }

        private static void DelayRunReleaseFile(string path, int time)
        {
            Task.Run(async () =>
            {
                await Task.Delay(time).ConfigureAwait(false);
                if (GetReferenceCount(path) == 0)
                {
                    if (m_pathStorage.TryRemove(path, out var fileStorage))
                    {
                        fileStorage.Dispose();
                    }
                }
            });
        }

        /// <summary>
        /// 减少引用次数，并尝试释放流。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="delayTime">延迟释放时间。当设置为0时，立即释放,单位毫秒。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static Result TryReleaseFile(string path, int delayTime = 0)
        {
            ThrowIfPathIsNull(path);
            path = Path.GetFullPath(path);
            if (m_pathStorage.TryGetValue(path, out var fileStorage))
            {
                if (Interlocked.Decrement(ref fileStorage.m_reference) <= 0)
                {
                    if (delayTime > 0)
                    {
                        DelayRunReleaseFile(path, delayTime);
                        return new Result(ResultCode.Success);
                    }
                    else
                    {
                        if (m_pathStorage.TryRemove(path, out fileStorage))
                        {
                            fileStorage.Dispose();
                        }
                        return new Result(ResultCode.Success);
                    }
                }
                else
                {
                    return new Result(ResultCode.Error, TouchSocketCoreResource.StreamReferencing.Format(path, fileStorage.m_reference));
                }
            }
            else
            {
                return new Result(ResultCode.Success, TouchSocketCoreResource.StreamNotFind.Format(path));
            }
        }

        private static void OnTimer(object state)
        {
            var keys = new List<string>();
            foreach (var item in m_pathStorage)
            {
                if (DateTime.UtcNow - item.Value.AccessTime > item.Value.AccessTimeout)
                {
                    keys.Add(item.Key);
                }
            }
            foreach (var item in keys)
            {
                TryReleaseFile(item);
            }
        }
    }
}