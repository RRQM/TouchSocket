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
using System.Collections.Concurrent;
using System.IO;

namespace RRQMSocket.FileTransfer
{
    internal static class TransferFileStreamDic
    {
        private static ConcurrentDictionary<string, FileStream> readOrWriteStreamDic = new ConcurrentDictionary<string, FileStream>();

        internal static FileStream GetFileStream(string path)
        {
            return readOrWriteStreamDic.GetOrAdd(path, (v) =>
              {
                  return File.OpenRead(path);
              });
        }

        internal static void DisposeFileStream(string path)
        {
            FileStream stream;
            if (readOrWriteStreamDic.TryRemove(path, out stream))
            {
                stream.Dispose();
            }
        }
    }
}