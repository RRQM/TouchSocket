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
using System.IO;
using System.Threading;

namespace TouchSocket.Core.IO
{
    /// <summary>
    /// 文件池。
    /// </summary>
    public static class FilePool
    {
        private static readonly object m_locker = new object();
        private static readonly ConcurrentDictionary<string, FileStorage> pathStream = new ConcurrentDictionary<string, FileStorage>();

        /// <summary>
        /// 获取一个文件读取访问器
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileStorageReader GetReader(string path)
        {
            lock (m_locker)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new System.ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
                }

                path = Path.GetFullPath(path);
                if (pathStream.TryGetValue(path, out FileStorage fileStorage))
                {
                    Interlocked.Increment(ref fileStorage.reference);
                    return new FileStorageReader(fileStorage);
                }
                else
                {
                    LoadFileForRead(path);
                    return GetReader(path);
                }
            }
        }

        /// <summary>
        /// 获取引用次数。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int GetReferenceCount(string path)
        {
            lock (m_locker)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return 0;
                }
                if (pathStream.TryGetValue(path, out FileStorage fileStorage))
                {
                    return fileStorage.reference;
                }
                return 0;
            }
        }

        /// <summary>
        /// 获取一个文件写入访问器
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="singleRef">单一访问，当为true时，只能有一个引用访问。</param>
        /// <returns></returns>
        public static FileStorageWriter GetWriter(string path, bool singleRef = false)
        {
            lock (m_locker)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new System.ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
                }

                path = Path.GetFullPath(path);
                if (pathStream.TryGetValue(path, out FileStorage fileStorage))
                {
                    if (singleRef && fileStorage.reference != 0)
                    {
                        throw new Exception("该流文件正在其他地方引用。");
                    }
                    Interlocked.Increment(ref fileStorage.reference);
                    return new FileStorageWriter(fileStorage, singleRef);
                }
                else
                {
                    LoadFileForWrite(path);
                    return GetWriter(path, singleRef);
                }
            }
        }

        /// <summary>
        /// 加载文件为缓存读取流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void LoadFileForCacheRead(string path)
        {
            lock (m_locker)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new System.ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
                }

                path = Path.GetFullPath(path);
                if (pathStream.TryGetValue(path, out FileStorage storage))
                {
                    if (storage.Access != FileAccess.Read || !storage.Cache)
                    {
                        throw new Exception("该路径的文件已经被加载为其他模式。");
                    }
                    return;
                }
                if (FileStorage.TryCreateCacheFileStorage(path, out FileStorage fileStorage, out string msg))
                {
                    pathStream.TryAdd(path, fileStorage);
                }
                else
                {
                    throw new Exception(msg);
                }
            }
        }

        /// <summary>
        /// 加载文件为读取流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void LoadFileForRead(string path)
        {
            lock (m_locker)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new System.ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
                }

                path = Path.GetFullPath(path);
                if (pathStream.TryGetValue(path, out FileStorage storage))
                {
                    if (storage.Access != FileAccess.Read)
                    {
                        throw new Exception("该路径的文件已经被加载为写入模式。");
                    }
                    return;
                }
                if (FileStorage.TryCreateFileStorage(path, FileAccess.Read, out FileStorage fileStorage, out string msg))
                {
                    pathStream.TryAdd(path, fileStorage);
                }
                else
                {
                    throw new Exception(msg);
                }
            }
        }

        /// <summary>
        /// 加载文件为读取流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void LoadFileForWrite(string path)
        {
            lock (m_locker)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new System.ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
                }

                path = Path.GetFullPath(path);
                if (pathStream.TryGetValue(path, out FileStorage storage))
                {
                    if (storage.Access != FileAccess.Write)
                    {
                        throw new Exception("该路径的文件已经被加载为读取模式。");
                    }
                    return;
                }
                if (FileStorage.TryCreateFileStorage(path, FileAccess.Write, out FileStorage fileStorage, out string msg))
                {
                    pathStream.TryAdd(path, fileStorage);
                }
                else
                {
                    throw new Exception(msg);
                }
            }
        }

        /// <summary>
        /// 减少引用次数，并尝试释放流。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Result TryReleaseFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(path)));
            }
            path = Path.GetFullPath(path);
            if (pathStream.TryGetValue(path, out FileStorage fileStorage))
            {
                Interlocked.Decrement(ref fileStorage.reference);
                if (fileStorage.reference <= 0)
                {
                    if (pathStream.TryRemove(path, out fileStorage))
                    {
                        fileStorage.Dispose();
                    }
                    return new Result(ResultCode.Success, "流成功释放。");
                }
                else
                {
                    return new Result(ResultCode.Error, ResType.StreamReferencing.GetDescription(path, fileStorage.reference));
                }
            }
            else
            {
                return new Result(ResultCode.Success, ResType.StreamNotFind.GetDescription(path));
            }
        }
    }
}