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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 文件池。
    /// </summary>
    [IntelligentCoder.AsyncMethodPoster(Flags = IntelligentCoder.MemberFlags.Public)]
    public static partial class FilePool
    {
        private static readonly object m_locker = new object();

        private static readonly ConcurrentDictionary<string, FileStorage> m_pathStorage = new ConcurrentDictionary<string, FileStorage>();

        private static readonly Timer m_timer;

        static FilePool()
        {
            m_timer = new Timer(OnTimer, null, 60000, 60000);
        }

        /// <summary>
        /// 获取所有的路径。
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllPaths()
        {
            return m_pathStorage.Keys.ToArray();
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
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
            }
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
            if (m_pathStorage.TryGetValue(fileInfo.FullName, out FileStorage storage))
            {
                if (storage.FileAccess != FileAccess.Read)
                {
                    throw new Exception("该路径的文件已经被加载为仅写入模式。");
                }
                Interlocked.Increment(ref storage.m_reference);
                return storage;
            }
            lock (m_locker)
            {
                storage = new FileStorage(fileInfo, FileAccess.Read);
                if (m_pathStorage.TryAdd(fileInfo.FullName, storage))
                {
                    Interlocked.Increment(ref storage.m_reference);
                    return storage;
                }
                else
                {
                    return GetFileStorageForRead(fileInfo);
                }
               
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
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
            }
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
            if (m_pathStorage.TryGetValue(fileInfo.FullName, out FileStorage storage))
            {
                if (storage.FileAccess != FileAccess.Write)
                {
                    throw new Exception("该路径的文件已经被加载为仅读取模式。");
                }
                Interlocked.Increment(ref storage.m_reference);
                return storage;
            }
            lock (m_locker)
            {
                storage = new FileStorage(fileInfo, FileAccess.Write);
                if (m_pathStorage.TryAdd(fileInfo.FullName, storage))
                {
                    Interlocked.Increment(ref storage.m_reference);
                    return storage;
                }
                else
                {
                    return GetFileStorageForWrite(fileInfo);
                }
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
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }
            if (m_pathStorage.TryGetValue(path, out FileStorage fileStorage))
            {
                return fileStorage.m_reference;
            }
            return 0;
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

        /// <summary>
        /// 加载文件为缓存读取流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static void LoadFileForCacheRead(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
            }

            path = Path.GetFullPath(path);
            if (m_pathStorage.TryGetValue(path, out FileStorage storage))
            {
                if (storage.FileAccess != FileAccess.Read || !storage.Cache)
                {
                    throw new Exception("该路径的文件已经被加载为其他模式。");
                }
                return;
            }
            if (FileStorage.TryCreateCacheFileStorage(path, out FileStorage fileStorage, out string msg))
            {
                m_pathStorage.TryAdd(path, fileStorage);
            }
            else
            {
                throw new Exception(msg);
            }
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
            if (string.IsNullOrEmpty(path))
            {
                return new Result(ResultCode.Error, TouchSocketStatus.ArgumentNull.GetDescription(nameof(path)));
            }
            path = Path.GetFullPath(path);
            if (m_pathStorage.TryGetValue(path, out FileStorage fileStorage))
            {
                if (Interlocked.Decrement(ref fileStorage.m_reference) <= 0)
                {
                    if (delayTime > 0)
                    {
                        EasyTask.DelayRun(delayTime, path, (p) =>
                        {
                            if (GetReferenceCount(p) == 0)
                            {
                                if (m_pathStorage.TryRemove((string)p, out fileStorage))
                                {
                                    fileStorage.Dispose();
                                }
                            }
                        });
                        return new Result(ResultCode.Success, $"如果在{delayTime}ms后引用仍然为0的话，即被释放。");
                    }
                    else
                    {
                        if (m_pathStorage.TryRemove(path, out fileStorage))
                        {
                            fileStorage.Dispose();
                        }
                        return new Result(ResultCode.Success, "流成功释放。");
                    }
                }
                else
                {
                    return new Result(ResultCode.Error, TouchSocketStatus.StreamReferencing.GetDescription(path, fileStorage.m_reference));
                }
            }
            else
            {
                return new Result(ResultCode.Success, TouchSocketStatus.StreamNotFind.GetDescription(path));
            }
        }

        private static void OnTimer(object state)
        {
            var keys = new List<string>();
            foreach (KeyValuePair<string, FileStorage> item in m_pathStorage)
            {
                if (DateTime.Now - item.Value.AccessTime > item.Value.AccessTimeout)
                {
                    keys.Add(item.Key);
                }
            }
            foreach (string item in keys)
            {
                TryReleaseFile(item);
            }
        }
    }
}