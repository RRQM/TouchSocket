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
using System.IO;
using TouchSocket.Core.IO;
using TouchSocket.Core.XREF.Newtonsoft.Json;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 文件
    /// </summary>
    internal static class FileTool
    {
        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="md5"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static void GetFileInfo<T>(string path, bool md5, ref T fileInfo) where T : RemoteFileInfo
        {
            FileInfo info = new FileInfo(path);
            fileInfo.FilePath = info.FullName;
            fileInfo.FileName = info.Name;
            fileInfo.Attributes = info.Attributes;
            fileInfo.CreationTime = info.CreationTime;
            fileInfo.FileLength = info.Length;
            fileInfo.LastAccessTime = info.LastAccessTime;
            fileInfo.LastWriteTime = info.LastWriteTime;

            if (md5)
            {
                using FileStream fileStream = File.OpenRead(path);
                fileInfo.MD5 = FileUtility.GetStreamMD5(fileStream);
            }
        }

        /// <summary>
        /// 读取缓存文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool TryReadTempInfo(string path, ref TouchRpcFileInfo info)
        {
            string filePath = path + ".rrqm";
            string tempPath = path + ".temp";
            if (File.Exists(filePath) && File.Exists(tempPath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    TouchRpcFileInfo tempInfo = JsonConvert.DeserializeObject<TouchRpcFileInfo>(File.ReadAllText(tempPath));
                    if (tempInfo.MD5 == info.MD5 && tempInfo.FileLength == info.FileLength)
                    {
                        info.Position = tempInfo.Position;
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}