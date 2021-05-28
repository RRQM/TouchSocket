//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.IO;
using System.Collections.Concurrent;
using System.IO;

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
        private static ConcurrentDictionary<string, FileInfo> filePathAndInfo = new ConcurrentDictionary<string, FileInfo>();

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
            if (!File.Exists(filePath))
            {
                return;
            }

            FileInfo fileInfo = new FileInfo();
            using (FileStream stream = File.OpenRead(filePath))
            {
                fileInfo.FilePath = filePath;
                fileInfo.FileLength = stream.Length;
                fileInfo.FileName = Path.GetFileName(filePath);
                fileInfo.FileHash = FileControler.GetStreamHash(stream);
            }
            AddFile(fileInfo);
        }

        /// <summary>
        /// 添加文件信息
        /// </summary>
        /// <param name="fileInfo"></param>
        public static void AddFile(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return;
            }
            filePathAndInfo.AddOrUpdate(fileInfo.FilePath, fileInfo, (key, oldValue) =>
              {
                  return fileInfo;
              });

            if (filePathAndInfo.Count > MaxCount)
            {
                foreach (var item in filePathAndInfo.Keys)
                {
                    FileInfo info;
                    if (filePathAndInfo.TryRemove(item, out info))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 清除全部
        /// </summary>
        public static void ClearDictionary()
        {
            if (filePathAndInfo == null)
            {
                return;
            }
            filePathAndInfo.Clear();
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool Remove(string filePath)
        {
            if (filePathAndInfo == null)
            {
                return false;
            }
            return filePathAndInfo.TryRemove(filePath, out _);
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileInfo"></param>
        /// <param name="breakpointResume"></param>
        /// <returns></returns>
        public static bool GetFileInfo(string filePath, out FileInfo fileInfo, bool breakpointResume)
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
                            if (breakpointResume && fileInfo.FileHash == null)
                            {
                                fileInfo.FileHash = FileControler.GetStreamHash(stream);
                                AddFile(fileInfo);
                            }
                            return true;
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

            fileInfo = null;
            return false;
        }
    }
}