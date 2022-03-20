//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.IO;
using System.IO;
using System.Security.Cryptography;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件Hash校验
    /// </summary>
    public static class FileTool
    {
        /// <summary>
        /// 获取文件Hash
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileCheckerType"></param>
        /// <returns></returns>
        public static string GetFileHashID(string path, FileCheckerType fileCheckerType)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return GetFileHashID(fileStream, fileCheckerType);
            }
        }

        /// <summary>
        /// 获取文件唯一ID
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="fileCheckerType"></param>
        /// <returns></returns>
        public static string GetFileHashID(FileStream fileStream, FileCheckerType fileCheckerType)
        {
            switch (fileCheckerType)
            {
                case FileCheckerType.MD5:
                    HashAlgorithm hash = MD5.Create();
                    return FileControler.GetStreamHash(fileStream, hash);

                case FileCheckerType.SHA1:
                    HashAlgorithm hashShA1 = SHA1.Create();
                    return FileControler.GetStreamHash(fileStream, hashShA1);

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileCheckerType"></param>
        /// <returns></returns>
        public static RRQMFileInfo GetFileInfo(string path, FileCheckerType fileCheckerType)
        {
            RRQMFileInfo fileInfo = new RRQMFileInfo();
            fileInfo.FilePath = path;
            fileInfo.FileName = Path.GetFileName(path);
            using (FileStream fileStream = File.OpenRead(path))
            {
                fileInfo.FileLength = fileStream.Length;
                fileInfo.FileHashID = GetFileHashID(fileStream, fileCheckerType);
            }

            return fileInfo;
        }

        /// <summary>
        /// 读取缓存文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool TryReadFileInfoFromPath(string path, out RRQMFileInfo fileInfo)
        {
            bool b = TryReadFileInfoFromPath(path, out fileInfo, out RRQMStream stream);
            stream.AbsoluteDispose();
            return b;
        }

        /// <summary>
        /// 读取缓存文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileInfo"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool TryReadFileInfoFromPath(string path, out RRQMFileInfo fileInfo, out RRQMStream stream)
        {
            string tempPath = path.Replace(".rrqm", string.Empty) + ".temp";
            if (File.Exists(path) && File.Exists(tempPath))
            {
                stream = new RRQMStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                try
                {
                    fileInfo = RRQMCore.Serialization.SerializeConvert.XmlDeserializeFromFile<RRQMFileInfo>(tempPath);
                    if (fileInfo == null || fileInfo.Posotion > stream.Length)
                    {
                        stream.AbsoluteDispose();
                        return false;
                    }
                    return true;
                }
                catch
                {
                    stream.AbsoluteDispose();
                    stream = null;
                    fileInfo = null;
                    return false;
                }
            }
            else
            {
                fileInfo = null;
                stream = null;
                return false;
            }
        }
    }
}