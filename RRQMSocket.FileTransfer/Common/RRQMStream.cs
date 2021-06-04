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
using System;
using System.IO;
using RRQMCore.Serialization;

namespace RRQMSocket.FileTransfer
{
    internal class RRQMStream:IDisposable
    {
        internal static RRQMStream GetRQMStream(ref ProgressBlockCollection blocks, bool restart, bool breakpoint)
        {
            RRQMStream stream = new RRQMStream();
            stream.fileInfo = blocks.FileInfo;
            string rrqmPath = blocks.FileInfo.FilePath + ".rrqm";
            string tempPath = blocks.FileInfo.FilePath + ".temp";

            if (File.Exists(rrqmPath) && File.Exists(tempPath) && !restart && breakpoint)
            {
                PBCollectionTemp readBlocks = SerializeConvert.RRQMBinaryDeserialize<PBCollectionTemp>(File.ReadAllBytes(rrqmPath));
                if (readBlocks.FileInfo.FileHash != null && blocks.FileInfo.FileHash != null && readBlocks.FileInfo.FileHash == blocks.FileInfo.FileHash)
                {
                    stream.tempFileStream = new FileStream(tempPath, FileMode.Open, FileAccess.ReadWrite);
                    stream.rrqmFileStream = new FileStream(rrqmPath, FileMode.Open, FileAccess.ReadWrite);
                    blocks = readBlocks.ToPBCollection();

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
                stream.rrqmFileStream.Write(dataBuffer,0,dataBuffer.Length);
                stream.rrqmFileStream.Flush();
            }

            stream.tempFileStream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite);
            return stream;
        }

       
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
            if (this.rrqmFileStream!=null)
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