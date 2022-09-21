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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace TouchSocket.Core.IO
{
    /// <summary>
    /// 文件操作
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// 获取不重复文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDuplicateFileName(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return fileName;
            }

            int index = 0;
            while (true)
            {
                index++;
                string newPath = Path.Combine(Path.GetDirectoryName(fileName), $"{Path.GetFileNameWithoutExtension(fileName)}({index}){Path.GetExtension(fileName)}");
                if (!File.Exists(newPath))
                {
                    return newPath;
                }
            }
        }

        /// <summary>
        /// 获取不重复文件夹名称
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public static string GetDuplicateDirectoryName(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                return dirName;
            }

            int index = 0;
            while (true)
            {
                index++;
                string newPath = Path.Combine(Path.GetDirectoryName(dirName), $"{Path.GetFileNameWithoutExtension(dirName)}({index})");
                if (!System.IO.Directory.Exists(newPath))
                {
                    return newPath;
                }
            }
        }

        /// <summary>
        /// 转化为文件大小的字符串，类似10B，10Kb，10Mb，10Gb。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToFileLengthString(long length)
        {
            if (length < 1024)
            {
                return $"{length}B";
            }
            else if (length < 1024 * 1024)
            {
                return $"{(length / 1024.0).ToString("0.00")}Kb";
            }
            else if (length < 1024 * 1024 * 1024)
            {
                return $"{(length / (1024.0 * 1024)).ToString("0.00")}Mb";
            }
            else
            {
                return $"{(length / (1024.0 * 1024 * 1024)).ToString("0.00")}Gb";
            }
        }

        /// <summary>
        /// 获取文件MD5
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileMD5(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return GetStreamMD5(fileStream);
            }
        }

        /// <summary>
        /// 获取流MD5
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static string GetStreamMD5(Stream fileStream)
        {
            using (HashAlgorithm hash = MD5.Create())
            {
                return GetStreamHash(fileStream, hash);
            }
        }

        /// <summary>
        /// 获得文件Hash值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string GetFileHash256(string filePath)
        {
            try
            {
                HashAlgorithm hash = SHA256.Create();
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    byte[] HashValue = hash.ComputeHash(fileStream);
                    return BitConverter.ToString(HashValue).Replace("-", "");
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获得流Hash值
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetStreamHash256(Stream stream)
        {
            try
            {
                HashAlgorithm hash = SHA256.Create();
                byte[] HashValue = hash.ComputeHash(stream);
                return BitConverter.ToString(HashValue).Replace("-", "");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获得文件Hash值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string GetFileHash(string filePath, HashAlgorithm hash)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    byte[] HashValue = hash.ComputeHash(fileStream);
                    return BitConverter.ToString(HashValue).Replace("-", "");
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获得流Hash值
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string GetStreamHash(Stream stream, HashAlgorithm hash)
        {
            try
            {
                byte[] HashValue = hash.ComputeHash(stream);
                return BitConverter.ToString(HashValue).Replace("-", "");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取文件夹下一级文件名称，不含路径。
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <returns></returns>
        public static string[] GetFiles(string sourceFolder)
        {
            return Directory.GetFiles(sourceFolder).Select(s => Path.GetFileName(s)).ToArray();
        }

# if NET45_OR_GREATER

        [DllImport("kernel32.dll")]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int OF_READWRITE = 2;

        private const int OF_SHARE_DENY_NONE = 0x40;

        private static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        /// <summary>
        /// 判断文件是否被已打开
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public static bool FileIsOpen(string fileFullName)
        {
            if (!File.Exists(fileFullName))
            {
                return false;
            }

            IntPtr handle = _lopen(fileFullName, OF_READWRITE | OF_SHARE_DENY_NONE);

            if (handle == HFILE_ERROR)
            {
                return true;
            }

            CloseHandle(handle);

            return false;
        }

#endif
    }
}