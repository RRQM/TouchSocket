//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件流池
    /// </summary>
    public static class FileStreamPool
    {
        internal static ConcurrentDictionary<string, RRQMStream> pathStream = new ConcurrentDictionary<string, RRQMStream>();
        private readonly static object locker = new object();
        private static ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        internal static bool CheckAllFileBlockFinished(string path)
        {
            lock (locker)
            {
                RRQMStream stream;
                if (!pathStream.TryGetValue(path, out stream))
                {
                    return false;
                }
                if (stream.StreamType == StreamOperationType.Read)
                {
                    return false;
                }
                foreach (var block in stream.Blocks)
                {
                    if (block.RequestStatus != RequestStatus.Finished)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal static void DisposeReadStream(string path)
        {
            RRQMStream stream;
            if (pathStream.TryGetValue(path, out stream))
            {
                if (Interlocked.Decrement(ref stream.reference) == 0)
                {
                    if (pathStream.TryRemove(path, out stream))
                    {
                        stream.Dispose();
                    }
                }
            }
        }

        internal static void DisposeWriteStream(string path, bool finished)
        {
            RRQMStream stream;
            if (pathStream.TryGetValue(path, out stream))
            {
                if (Interlocked.Decrement(ref stream.reference) == 0)
                {
                    if (pathStream.TryRemove(path, out stream))
                    {
                        if (finished)
                        {
                            stream.FinishStream();
                        }
                        else
                        {
                            stream.Dispose();
                        }
                    }
                }
            }
        }

        internal static bool GetFreeFileBlock(string path, out FileBlock fileBlock, out string mes)
        {
            lock (locker)
            {
                RRQMStream stream;
                if (!pathStream.TryGetValue(path, out stream))
                {
                    mes = "没有此路径的写入信息";
                    fileBlock = null;
                    return false;
                }
                if (stream.StreamType == StreamOperationType.Read)
                {
                    mes = "该路径的流为只读";
                    fileBlock = null;
                    return false;
                }
                foreach (var block in stream.Blocks)
                {
                    if (block.RequestStatus == RequestStatus.Hovering)
                    {
                        block.RequestStatus = RequestStatus.InProgress;
                        fileBlock = block;
                        mes = null;
                        return true;
                    }
                }
                fileBlock = null;
                mes = null;
                return true;
            }
        }

        internal static bool LoadReadStream(ref UrlFileInfo urlFileInfo, out string mes)
        {
            RRQMStream stream;
            if (pathStream.TryGetValue(urlFileInfo.FilePath, out stream))
            {
                Interlocked.Increment(ref stream.reference);
                mes = null;
                return true;
            }
            else
            {
                if (RRQMStream.CreateReadStream(out stream, ref urlFileInfo, out mes))
                {
                    Interlocked.Increment(ref stream.reference);
                    pathStream.TryAdd(urlFileInfo.FilePath, stream);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal static bool LoadWriteStream(ref ProgressBlockCollection blocks, bool onlySearch, out string mes)
        {
            RRQMStream stream;
            string rrqmPath = blocks.UrlFileInfo.SaveFullPath + ".rrqm";
            if (!pathStream.TryGetValue(rrqmPath, out stream))
            {
                if (RRQMStream.CreateWriteStream(out stream, ref blocks, out mes))
                {
                    Interlocked.Increment(ref stream.reference);
                    mes = null;
                    return pathStream.TryAdd(rrqmPath, stream);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (onlySearch)
                {
                    blocks = stream.Blocks;
                    mes = null;
                    return true;
                }
                mes = "该文件流正在被其他客户端拥有";
                return false;
            }
        }

        internal static bool ReadFile(string path, out string mes, long beginPosition, ByteBlock byteBlock, int offset, int length)
        {
            lockSlim.EnterReadLock();
            try
            {
                if (pathStream.TryGetValue(path, out RRQMStream stream))
                {
                    stream.FileStream.Position = beginPosition;

                    if (byteBlock.Buffer.Length < length + offset)
                    {
                        byteBlock.SetBuffer(new byte[length + offset]);
                    }

                    int r = stream.FileStream.Read(byteBlock.Buffer, offset, length);
                    if (r == length)
                    {
                        byteBlock.Position = offset + length;
                        byteBlock.SetLength(offset + length);
                        mes = null;
                        return true;
                    }
                }
                mes = "没有找到该路径下的流文件";
                return false;
            }
            catch (Exception ex)
            {
                mes = ex.Message;
                return false;
            }
            finally
            {
                lockSlim.ExitReadLock();
            }
        }

        internal static void SaveProgressBlockCollection(string path)
        {
            RRQMStream stream;
            if (pathStream.TryGetValue(path, out stream))
            {
                stream.SaveProgressBlockCollection();
            }
            else
            {
                throw new RRQMException("没有找到该路径下的流文件");
            }
        }

        internal static bool WriteFile(string path, out string mes, out RRQMStream stream, long streamPosition, byte[] buffer, int offset, int length)
        {
            try
            {
                if (pathStream.TryGetValue(path, out stream))
                {
                    stream.FileStream.Position = streamPosition;
                    stream.FileStream.Write(buffer, offset, length);
                    stream.FileStream.Flush();
                    mes = null;
                    return true;
                }
                mes = "未找到该路径下的流";
                return false;
            }
            catch (Exception ex)
            {
                mes = ex.Message;
                stream = null;
                return false;
            }
        }
    }
}