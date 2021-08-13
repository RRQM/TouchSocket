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
using RRQMCore.Serialization;
using System;
using System.IO;

namespace RRQMSocket.FileTransfer
{
    internal class RRQMStream
    {
        internal int reference;

        private ProgressBlockCollection blocks;

        private FileStream fileStream;
        private string rrqmPath;
        private StreamOperationType streamType;

        private UrlFileInfo urlFileInfo;

        private RRQMStream()
        {
        }

        public ProgressBlockCollection Blocks { get { return blocks; } }

        public FileStream FileStream
        {
            get { return fileStream; }
        }

        public StreamOperationType StreamType
        {
            get { return streamType; }
        }

        public UrlFileInfo UrlFileInfo { get => urlFileInfo; }

        internal static bool CreateReadStream(out RRQMStream stream, ref UrlFileInfo urlFileInfo, out string mes)
        {
            stream = new RRQMStream();
            try
            {
                stream.streamType = StreamOperationType.Read;
                stream.fileStream = File.OpenRead(urlFileInfo.FilePath);
                urlFileInfo.FileLength = stream.fileStream.Length;
                stream.urlFileInfo = urlFileInfo;
                mes = null;
                return true;
            }
            catch (Exception ex)
            {
                stream.Dispose();
                stream = null;
                mes = ex.Message;
                return false;
            }
        }

        internal static bool CreateWriteStream(out RRQMStream stream, ref ProgressBlockCollection blocks, out string mes)
        {
            stream = new RRQMStream();
            stream.rrqmPath = blocks.UrlFileInfo.SaveFullPath + ".rrqm";
            stream.urlFileInfo = blocks.UrlFileInfo;
            stream.streamType = StreamOperationType.Write;
            try
            {
                if (blocks.UrlFileInfo.Flags.HasFlag(TransferFlags.BreakpointResume) && File.Exists(stream.rrqmPath))
                {
                    stream.fileStream = new FileStream(stream.rrqmPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    int blocksLength = (int)(stream.fileStream.Length - blocks.UrlFileInfo.FileLength);
                    if (blocksLength > 0)
                    {
                        stream.fileStream.Position = blocks.UrlFileInfo.FileLength;
                        byte[] buffer = new byte[blocksLength];
                        stream.fileStream.Read(buffer, 0, buffer.Length);
                        try
                        {
                            PBCollectionTemp readBlocks = SerializeConvert.RRQMBinaryDeserialize<PBCollectionTemp>(buffer);
                            if (readBlocks.UrlFileInfo != null && blocks.UrlFileInfo != null && readBlocks.UrlFileInfo.FileHash != null)
                            {
                                if (readBlocks.UrlFileInfo.FileHash == blocks.UrlFileInfo.FileHash)
                                {
                                    stream.blocks = blocks = readBlocks.ToPBCollection();
                                    mes = null;
                                    return true;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    stream.fileStream.Dispose();
                }
                stream.blocks = blocks;
                if (File.Exists(stream.rrqmPath))
                {
                    File.Delete(stream.rrqmPath);
                }
                stream.fileStream = new FileStream(stream.rrqmPath, FileMode.Create, FileAccess.ReadWrite);
                stream.SaveProgressBlockCollection();
                mes = null;
                return true;
            }
            catch (Exception ex)
            {
                mes = ex.Message;
                stream.Dispose();
                stream = null;
                return false;
            }
        }

        internal bool FinishStream()
        {
            this.fileStream.SetLength(this.urlFileInfo.FileLength);
            this.fileStream.Flush();
            UrlFileInfo info = this.urlFileInfo;
            this.Dispose();
            if (File.Exists(info.SaveFullPath))
            {
                File.Delete(info.SaveFullPath);
            }
            File.Move(info.SaveFullPath + ".rrqm", info.SaveFullPath);
            return true;
        }

        internal void Dispose()
        {
            this.blocks = null;
            this.urlFileInfo = null;
            if (this.fileStream != null)
            {
                this.fileStream.Dispose();
                this.fileStream = null;
            }
        }

        internal void SaveProgressBlockCollection()
        {
            byte[] dataBuffer = SerializeConvert.RRQMBinarySerialize(PBCollectionTemp.GetFromProgressBlockCollection(blocks), true);
            this.fileStream.Position = this.urlFileInfo.FileLength;
            this.fileStream.WriteAsync(dataBuffer, 0, dataBuffer.Length);
            this.fileStream.Flush();
        }
    }
}