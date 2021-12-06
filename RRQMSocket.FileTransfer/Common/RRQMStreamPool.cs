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
using RRQMCore;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件流池
    /// </summary>
    public static class RRQMStreamPool
    {
        private static readonly ConcurrentDictionary<string, RRQMStream> pathStream = new ConcurrentDictionary<string, RRQMStream>();

        /// <summary>
        /// 完成读取信号
        /// </summary>
        /// <param name="path"></param>
        public static IResult FinishedReadStream(string path)
        {
            RRQMStream stream;
            if (pathStream.TryGetValue(path, out stream))
            {
                lock (stream)
                {
                    stream.reference--;
                    if (stream.reference == 0)
                    {
                        return TryReleaseReadStream(path);
                    }
                    else
                    {
                        return new Result(ResultCode.Success);
                    }
                }
            }
            else
            {
                return new Result(ResultCode.Error, ResType.NotFindStream.GetResString(path));
            }
        }

        /// <summary>
        /// 加载读取流
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileInfo"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public static bool LoadReadStream(string path, out RRQMFileInfo fileInfo, out string mes)
        {
            lock (typeof(RRQMStreamPool))
            {
                RRQMStream stream;
                if (pathStream.TryGetValue(path, out stream))
                {
                    stream.reference++;
                    mes = null;
                    fileInfo = stream.FileInfo;
                    return true;
                }
                else
                {
                    try
                    {
                        CreateReadStream(path, out fileInfo, out stream);
                        pathStream.TryAdd(path, stream);
                        mes = null;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        fileInfo = null;
                        mes = ex.Message;
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 加载写入流
        /// </summary>
        /// <param name="path"></param>
        /// <param name="owner"></param>
        /// <param name="flags"></param>
        /// <param name="fileInfo"></param>
        /// <param name="stream"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public static bool LoadWriteStream(string path, object owner, TransferFlags flags, ref RRQMFileInfo fileInfo, out RRQMStream stream, out string mes)
        {
            lock (typeof(RRQMStreamPool))
            {
                if (pathStream.TryGetValue(path, out stream))
                {
                    stream = null;
                    mes = "该流数据正在被其他传输占用。";
                    return false;
                }
                else
                {
                    try
                    {
                        CreateWriteStream(path, owner, flags, ref fileInfo, out stream);
                        pathStream.TryAdd(path, stream);
                        mes = null;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (stream != null)
                        {
                            stream.AbsoluteDispose();
                        }
                        fileInfo = null;
                        mes = ex.Message;
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 读取字节,线程安全
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mes"></param>
        /// <param name="beginPosition"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int ReadBytes(string path, out string mes, long beginPosition, byte[] buffer, int offset, int length)
        {
            if (pathStream.TryGetValue(path, out RRQMStream stream))
            {
                lock (stream)
                {
                    stream.Position = beginPosition;
                    int r = stream.Read(buffer, offset, length);
                    mes = null;
                    return r;
                }
            }
            else
            {
                mes = "没有找到该路径下的流文件";
                return -1;
            }
        }

        /// <summary>
        /// 移除流的拥有信息，从而可以调用<see cref="TryReleaseWriteStream"/>,
        /// 然后绝对释放流。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IResult RemoveWriteStreamOwner(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetResString(nameof(path)));
            }

            if (pathStream.TryGetValue(path, out RRQMStream stream))
            {
                stream.owner = null;
                return new Result(ResultCode.Success);
            }
            else
            {
                return new Result(ResultCode.Error, ResType.NotFindStream.GetResString(path));
            }
        }

        /// <summary>
        /// 尝试释放流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IResult TryReleaseReadStream(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetResString(nameof(path)));
            }

            RRQMStream stream;
            if (pathStream.TryGetValue(path, out stream))
            {
                if (stream.reference == 0)
                {
                    if (pathStream.TryRemove(path, out stream))
                    {
                        stream.AbsoluteDispose();
                    }
                    return new Result(ResultCode.Success, "流成功释放。");
                }
                else
                {
                    return new Result(ResultCode.Error, ResType.StreamReferencing.GetResString(path, stream.reference));
                }
            }
            else
            {
                return new Result(ResultCode.Error, ResType.NotFindStream.GetResString(path));
            }
        }
        /// <summary>
        /// 释放写入流
        /// </summary>
        /// <param name="path"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static IResult TryReleaseWriteStream(string path, object owner)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetResString(nameof(path)));
            }

            if (pathStream.TryGetValue(path, out RRQMStream stream))
            {
                if (stream.owner == null || stream.owner == owner)
                {
                    if (pathStream.TryRemove(path, out stream))
                    {
                        stream.AbsoluteDispose();
                    }
                    return new Result(ResultCode.Success, "流成功释放。");
                }
                else
                {
                    return new Result(ResultCode.Error, "流拥有者不正确");
                }
            }
            else
            {
                return new Result(ResultCode.Error, ResType.NotFindStream.GetResString(path));
            }
        }

        private static void CreateReadStream(string path, out RRQMFileInfo fileInfo, out RRQMStream stream)
        {
            stream = new RRQMStream(path, FileMode.Open, FileAccess.Read);
            stream.streamType = StreamOperationType.Read;

            fileInfo = new RRQMFileInfo();
            fileInfo.FilePath = path;
            fileInfo.FileLength = stream.Length;
            fileInfo.FileName = Path.GetFileName(path);
            stream.fileInfo = fileInfo;
        }

        private static void CreateWriteStream(string path, object owner, TransferFlags flags, ref RRQMFileInfo fileInfo, out RRQMStream stream)
        {
            if (flags.HasFlag(TransferFlags.BreakpointResume))
            {
                if (FileTool.TryReadFileInfoFromPath(path, out RRQMFileInfo info, out stream))
                {
                    if (!string.IsNullOrEmpty(fileInfo.FileHashID) && (info.FileHashID == fileInfo.FileHashID))
                    {
                        stream.owner = owner;
                        fileInfo.Posotion = info.Posotion;
                        stream.fileInfo = fileInfo;
                        stream.streamType = StreamOperationType.RRQMWrite;
                        stream.rrqmPath = path;
                        return;
                    }
                    else
                    {
                        stream.AbsoluteDispose();
                    }
                }
                stream = new RRQMStream(path, FileMode.Create, FileAccess.ReadWrite);
                stream.owner = owner;
                stream.streamType = StreamOperationType.RRQMWrite;
                stream.fileInfo = fileInfo;
                stream.rrqmPath = path;
            }
            else
            {
                stream = new RRQMStream(path, FileMode.Create, FileAccess.ReadWrite);
                stream.owner = owner;
                stream.streamType = StreamOperationType.Write;
                stream.fileInfo = fileInfo;
                stream.rrqmPath = path;
            }
        }
    }
}