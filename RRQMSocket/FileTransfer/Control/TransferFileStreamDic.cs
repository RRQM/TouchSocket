using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    internal static class TransferFileStreamDic
    {
        static ConcurrentDictionary<string, FileStream> readOrWriteStreamDic = new ConcurrentDictionary<string, FileStream>();

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
