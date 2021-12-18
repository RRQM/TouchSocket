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
using System.IO;

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