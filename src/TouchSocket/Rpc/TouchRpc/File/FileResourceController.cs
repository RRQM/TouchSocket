
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 文件资源控制器。
    /// </summary>
    public class FileResourceController : IFileResourceController
    {
        /// <summary>
        /// 文件资源控制器
        /// </summary>
        public FileResourceController()
        {
            Timeout = TimeSpan.FromSeconds(60);
        }

        /// <summary>
        /// 超时时间。默认60秒。
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <inheritdoc/>
        public virtual void GetFileInfo<T>(string path, bool md5, ref T fileInfo) where T : RemoteFileInfo
        {
            FileInfo info = new FileInfo(path);
            fileInfo.FullName = info.FullName;
            fileInfo.Name = info.Name;
            fileInfo.Attributes = info.Attributes;
            fileInfo.CreationTime = info.CreationTime;
            fileInfo.Length = info.Length;
            fileInfo.LastAccessTime = info.LastAccessTime;
            fileInfo.LastWriteTime = info.LastWriteTime;

            if (md5)
            {
                using FileStream fileStream = File.OpenRead(path);
                fileInfo.MD5 = FileUtility.GetStreamMD5(fileStream);
            }
        }

        /// <inheritdoc/>
        public virtual string GetFullPath(string root, string path)
        {
            if (path.IsNullOrEmpty())
            {
                return string.Empty;
            }
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                return Path.GetFullPath(Path.Combine(root, path));
            }
        }

        /// <inheritdoc/>
        public virtual bool TryReadTempInfo(string path, TransferFlags flags, ref TouchRpcFileInfo info)
        {
            if (flags== TransferFlags.None)
            {
                return false;
            }
            string filePath = path + ".rrqm";
            string tempPath = path + ".temp";
            if (File.Exists(filePath) && File.Exists(tempPath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    TouchRpcFileInfo tempInfo = SerializeConvert.JsonDeserializeFromString<TouchRpcFileInfo>(File.ReadAllText(tempPath));
                    if (flags.HasFlag(TransferFlags.BreakpointResume))
                    {
                        if (tempInfo.Length != info.Length)
                        {
                            return false;
                        }
                        if (flags.HasFlag(TransferFlags.MD5Verify))
                        {
                            if ((tempInfo.MD5 == info.MD5 && info.MD5.HasValue()))
                            {
                                info.Position = tempInfo.Position;
                                return true;
                            }
                        }
                        else
                        {
                            info.Position = tempInfo.Position;
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual int ReadAllBytes(FileInfo fileInfo, byte[] buffer)
        {
            using (FileStorageReader reader = FilePool.GetReader(fileInfo))
            {
                return reader.Read(buffer, 0, buffer.Length);
            }
        }

        /// <inheritdoc/>
        public virtual void WriteAllBytes(FileInfo fileInfo, byte[] buffer, int offset, int length)
        {
            FileUtility.Delete(fileInfo.FullName);
            using (var writer = FilePool.GetWriter(fileInfo))
            {
                writer.Write(buffer, offset, length);
            }
        }
    }
}