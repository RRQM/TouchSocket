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
using RRQMCore.IO;
using System.IO;
using System.Security.Cryptography;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件Hash校验
    /// </summary>
    public static class FileHashGenerator
    {
        private static FileHashType fileCheckType;

        /// <summary>
        /// 文件
        /// </summary>
        public static FileHashType FileCheckType
        {
            get { return fileCheckType; }
            set { fileCheckType = value; }
        }

        /// <summary>
        /// 获取文件Hash
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileHash(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return GetFileHash(fileStream);
            }
        }

        /// <summary>
        /// 获取文件Hash
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static string GetFileHash(FileStream fileStream)
        {
            HashAlgorithm hash;
            switch (fileCheckType)
            {
                case FileHashType.MD5:
                    hash = MD5.Create();
                    break;

                case FileHashType.SHA1:
                    hash = SHA1.Create();
                    break;

                case FileHashType.SHA256:
                    hash = SHA256.Create();
                    break;

                case FileHashType.SHA512:
                    hash = SHA512.Create();
                    break;

                default:
                    hash = null;
                    break;
            }
            return FileControler.GetStreamHash(fileStream, hash);
        }
    }
}