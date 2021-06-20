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
using RRQMCore.Serialization;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace RRQMSocket.FileTransfer
{
    internal class RRQMStream : IDisposable
    {
        internal static ConcurrentDictionary<string, RRQMStream> pathAndStream = new ConcurrentDictionary<string, RRQMStream>();

        internal static RRQMStream GetRRQMStream(ref ProgressBlockCollection blocks, bool restart, bool breakpoint)
        {
            string rrqmPath = blocks.UrlFileInfo.FilePath + ".rrqm";
            string tempPath = blocks.UrlFileInfo.FilePath + ".temp";

            if (pathAndStream.TryRemove(blocks.UrlFileInfo.FilePath, out RRQMStream rrqmStream))
            {
                rrqmStream.Dispose();
            }
            RRQMStream stream = new RRQMStream();
            pathAndStream.TryAdd(blocks.UrlFileInfo.FilePath, stream);
            stream.fileInfo = blocks.UrlFileInfo;


            if (File.Exists(rrqmPath) && File.Exists(tempPath) && !restart && breakpoint)
            {
                PBCollectionTemp readBlocks = SerializeConvert.RRQMBinaryDeserialize<PBCollectionTemp>(File.ReadAllBytes(rrqmPath));
                if (readBlocks.UrlFileInfo.FileHash != null && blocks.UrlFileInfo.FileHash != null && readBlocks.UrlFileInfo.FileHash == blocks.UrlFileInfo.FileHash)
                {
                    stream.tempFileStream = new FileStream(tempPath, FileMode.Open, FileAccess.ReadWrite);
                    stream.rrqmFileStream = new FileStream(rrqmPath, FileMode.Open, FileAccess.ReadWrite);
                    stream.blocks = blocks = readBlocks.ToPBCollection();

                    return stream;
                }
            }

            if (File.Exists(rrqmPath))
            {
                File.Delete(rrqmPath);
            }
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            if (breakpoint)
            {
                byte[] dataBuffer = SerializeConvert.RRQMBinarySerialize(PBCollectionTemp.GetFromProgressBlockCollection(blocks), true);
                stream.rrqmFileStream = new FileStream(rrqmPath, FileMode.Create, FileAccess.ReadWrite);
                stream.rrqmFileStream.Write(dataBuffer, 0, dataBuffer.Length);
                stream.rrqmFileStream.Flush();
            }

            stream.tempFileStream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite);
            return stream;
        }

        internal static void FileFinished(RRQMStream stream)
        {
            stream.Dispose();
            string path = stream.fileInfo.FilePath;
            if (File.Exists(path + ".rrqm"))
            {
                File.Delete(path + ".rrqm");
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (File.Exists(path + ".temp"))
            {
                File.Move(path + ".temp", path);
            }
           
        }

        ProgressBlockCollection blocks;

        internal FileInfo fileInfo;

        private FileStream rrqmFileStream;

        public FileStream RRQMFileStream
        {
            get { return rrqmFileStream; }
        }

        private FileStream tempFileStream;

        public FileStream TempFileStream
        {
            get { return tempFileStream; }
        }

        public void Dispose()
        {
            string path = this.fileInfo.FilePath;
            pathAndStream.TryRemove(path, out _);
            this.blocks = null;
            if (this.rrqmFileStream != null)
            {
                this.rrqmFileStream.Dispose();
                this.rrqmFileStream = null;
            }
            if (this.tempFileStream != null)
            {
                this.tempFileStream.Dispose();
                this.tempFileStream = null;
            }
        }
    }
}