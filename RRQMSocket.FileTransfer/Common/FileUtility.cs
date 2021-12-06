using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件传输通用
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
            if (!System.IO.File.Exists(fileName))
            {
                return fileName;
            }

            int index = 0;
            while (true)
            {
                index++;
                string newPath = Path.Combine(Path.GetDirectoryName(fileName), $"{Path.GetFileNameWithoutExtension(fileName)}({index}){Path.GetExtension(fileName)}");
                if (!System.IO.File.Exists(newPath))
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
    }
}
