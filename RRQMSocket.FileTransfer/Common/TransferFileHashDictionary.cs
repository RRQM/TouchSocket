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
//using System.Collections.Concurrent;
//using System.IO;

//namespace RRQMSocket.FileTransfer
//{
//    /// <summary>
//    /// 传输文件Hash暂存字典
//    /// </summary>
//    public static class TransferFileHashDictionary
//    {
//        private static ConcurrentDictionary<string, RRQMFileInfo> fileHashAndInfo = new ConcurrentDictionary<string, RRQMFileInfo>();
//        private static ConcurrentDictionary<string, RRQMFileInfo> filePathAndInfo = new ConcurrentDictionary<string, RRQMFileInfo>();

//        /// <summary>
//        /// 字典存储文件Hash的最大数量，默认为10000
//        /// </summary>
//        public static int MaxCount { get; set; } = 10000;

//        /// <summary>
//        /// 添加文件信息
//        /// </summary>
//        /// <param name="filePath"></param>
//        /// <param name="breakpointResume"></param>
//        /// <returns></returns>
//        public static RRQMFileInfo AddFile(string filePath, bool breakpointResume = true)
//        {
//            RRQMFileInfo urlFileInfo = new RRQMFileInfo();
//            using (FileStream stream = File.OpenRead(filePath))
//            {
//                urlFileInfo.FilePath = filePath;
//                urlFileInfo.FileLength = stream.Length;
//                urlFileInfo.FileName = Path.GetFileName(filePath);
//                if (breakpointResume)
//                {
//                    //urlFileInfo.FileHash = FileChecker.GetFileHash(stream);
//                }
//            }
//            AddFile(urlFileInfo);
//            return urlFileInfo;
//        }

//        /// <summary>
//        /// 添加文件信息
//        /// </summary>
//        /// <param name="urlFileInfo"></param>
//        public static void AddFile(RRQMFileInfo urlFileInfo)
//        {
//            if (urlFileInfo == null)
//            {
//                return;
//            }
//            filePathAndInfo.AddOrUpdate(urlFileInfo.FilePath, urlFileInfo, (key, oldValue) =>
//              {
//                  return urlFileInfo;
//              });

//            if (!string.IsNullOrEmpty(urlFileInfo.FileHash))
//            {
//                fileHashAndInfo.AddOrUpdate(urlFileInfo.FileHash, urlFileInfo, (key, oldValue) =>
//                {
//                    return urlFileInfo;
//                });
//            }

//            if (filePathAndInfo.Count > MaxCount)
//            {
//                foreach (var item in filePathAndInfo.Keys)
//                {
//                    if (filePathAndInfo.TryRemove(item, out _))
//                    {
//                        break;
//                    }
//                }
//            }

//            if (fileHashAndInfo.Count > MaxCount)
//            {
//                foreach (var item in fileHashAndInfo.Keys)
//                {
//                    if (fileHashAndInfo.TryRemove(item, out _))
//                    {
//                        break;
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 清除全部
//        /// </summary>
//        public static void ClearDictionary()
//        {
//            if (filePathAndInfo == null)
//            {
//                return;
//            }
//            filePathAndInfo.Clear();
//        }

//        /// <summary>
//        /// 获取文件信息
//        /// </summary>
//        /// <param name="filePath"></param>
//        /// <param name="urlFileInfo"></param>
//        /// <param name="breakpointResume"></param>
//        /// <returns></returns>
//        public static bool GetFileInfo(string filePath, out RRQMFileInfo urlFileInfo, bool breakpointResume)
//        {
//            if (filePathAndInfo == null)
//            {
//                urlFileInfo = null;
//                return false;
//            }
//            if (filePathAndInfo.ContainsKey(filePath))
//            {
//                urlFileInfo = filePathAndInfo[filePath];
//                if (File.Exists(filePath))
//                {
//                    using (FileStream stream = File.OpenRead(filePath))
//                    {
//                        if (urlFileInfo.FileLength == stream.Length)
//                        {
//                            if (breakpointResume && urlFileInfo.FileHash == null)
//                            {
//                                //urlFileInfo.FileHash = FileChecker.GetFileHash(stream);
//                                AddFile(urlFileInfo);
//                            }
//                            return true;
//                        }
//                    }
//                }
//            }

//            urlFileInfo = null;
//            return false;
//        }

//        /// <summary>
//        /// 通过FileHash获取文件信息
//        /// </summary>
//        /// <param name="fileHash"></param>
//        /// <param name="urlFileInfo"></param>
//        /// <returns></returns>
//        public static bool GetFileInfoFromHash(string fileHash, out RRQMFileInfo urlFileInfo)
//        {
//            if (fileHashAndInfo == null)
//            {
//                urlFileInfo = null;
//                return false;
//            }
//            if (string.IsNullOrEmpty(fileHash))
//            {
//                urlFileInfo = null;
//                return false;
//            }

//            if (fileHashAndInfo.TryGetValue(fileHash, out urlFileInfo))
//            {
//                if (urlFileInfo.FileHash == fileHash)
//                {
//                    if (File.Exists(urlFileInfo.FilePath))
//                    {
//                        using (FileStream stream = File.OpenRead(urlFileInfo.FilePath))
//                        {
//                            if (urlFileInfo.FileLength == stream.Length)
//                            {
//                                return true;
//                            }
//                        }
//                    }
//                }
//            }

//            urlFileInfo = null;
//            return false;
//        }

//        /// <summary>
//        /// 移除
//        /// </summary>
//        /// <param name="filePath"></param>
//        /// <returns></returns>
//        public static bool Remove(string filePath)
//        {
//            if (filePathAndInfo == null)
//            {
//                return false;
//            }
//            return filePathAndInfo.TryRemove(filePath, out _);
//        }
//    }
//}