//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RRQMSocket.FileTransfer
{
    /*
    若汝棋茗
    */

    /// <summary>
    /// 传输文件Hash暂存字典
    /// </summary>
    public static class TransferFileHashDictionary
    {
        internal static void Initialization()
        {
            if (filePathAndInfo == null)
            {
                filePathAndInfo = new Dictionary<string, FileInfo>();
            }
        }

        internal static void UnInitialization()
        {
            if (filePathAndInfo != null)
            {
                filePathAndInfo.Clear();
                filePathAndInfo = null;
            }
        }

        private static Dictionary<string, FileInfo> filePathAndInfo;

        /// <summary>
        /// 字典存储文件Hash的最大数量，默认为5000
        /// </summary>
        public static int MaxCount { get; set; } = 5000;

        /// <summary>
        /// 添加文件信息
        /// </summary>
        /// <param name="filePath"></param>
        public static void AddFile(string filePath)
        {
            lock (typeof(TransferFileHashDictionary))
            {
                if (filePathAndInfo == null)
                {
                    return;
                }
                FileInfo fileInfo = new FileInfo();
                using (FileStream stream = File.OpenRead(filePath))
                {
                    fileInfo.FilePath = filePath;
                    fileInfo.FileLength = stream.Length;
                    fileInfo.FileName = Path.GetFileName(filePath);
                    fileInfo.FileHash =FileControler.GetStreamHash(stream);
                }
                if (filePathAndInfo.Keys.Contains(filePath))
                {
                    filePathAndInfo[filePath] = fileInfo;
                }
                else
                {
                    lock (typeof(TransferFileHashDictionary))
                    {
                        filePathAndInfo.Add(filePath, fileInfo);

                        if (filePathAndInfo.Count > MaxCount)
                        {
                            foreach (var item in filePathAndInfo.Keys)
                            {
                                filePathAndInfo.Remove(item);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加文件信息
        /// </summary>
        /// <param name="fileInfo"></param>
        public static void AddFile(FileInfo fileInfo)
        {
            lock (typeof(TransferFileHashDictionary))
            {
                if (filePathAndInfo == null)
                {
                    return;
                }

                if (filePathAndInfo.Keys.Contains(fileInfo.FilePath))
                {
                    filePathAndInfo[fileInfo.FilePath] = fileInfo;
                }
                else
                {
                    filePathAndInfo.Add(fileInfo.FilePath, fileInfo);
                    if (filePathAndInfo.Count > MaxCount)
                    {
                        foreach (var item in filePathAndInfo.Keys)
                        {
                            filePathAndInfo.Remove(item);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清除全部
        /// </summary>
        public static void ClearDictionary()
        {
            lock (typeof(TransferFileHashDictionary))
            {
                if (filePathAndInfo == null)
                {
                    return;
                }
                filePathAndInfo.Clear();
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="filePath"></param>
        public static void Remove(string filePath)
        {
            lock (typeof(TransferFileHashDictionary))
            {
                if (filePathAndInfo == null)
                {
                    return;
                }
                if (filePathAndInfo.ContainsKey(filePath))
                {
                    filePathAndInfo.Remove(filePath);
                }
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool GetFileInfo(string filePath, out FileInfo fileInfo)
        {
            lock (typeof(TransferFileHashDictionary))
            {
                if (filePathAndInfo == null)
                {
                    fileInfo = null;
                    return false;
                }
                if (filePathAndInfo.ContainsKey(filePath))
                {
                    fileInfo = filePathAndInfo[filePath];
                    if (File.Exists(filePath))
                    {
                        using (FileStream stream = File.OpenRead(filePath))
                        {
                            if (fileInfo.FileLength == stream.Length)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            fileInfo = null;
            return false;
        }

        /// <summary>
        /// 通过FileHash获取文件信息
        /// </summary>
        /// <param name="fileHash"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool GetFileInfoFromHash(string fileHash, out FileInfo fileInfo)
        {
            lock (typeof(TransferFileHashDictionary))
            {
                if (filePathAndInfo == null)
                {
                    fileInfo = null;
                    return false;
                }
                if (fileHash == null)
                {
                    fileInfo = null;
                    return false;
                }

                foreach (var item in filePathAndInfo.Values)
                {
                    if (item.FileHash == fileHash)
                    {
                        if (File.Exists(item.FilePath))
                        {
                            using (FileStream stream = File.OpenRead(item.FilePath))
                            {
                                if (item.FileLength == stream.Length)
                                {
                                    fileInfo = item;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            fileInfo = null;
            return false;
        }
    }
}