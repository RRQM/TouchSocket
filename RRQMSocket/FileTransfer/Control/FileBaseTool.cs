//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.IO;
using RRQMCore.ByteManager;
using RRQMCore.Serialization;

namespace RRQMSocket.FileTransfer
{
    /*
    若汝棋茗
    */

    internal static class FileBaseTool
    {
        #region Methods

        /// <summary>
        /// 创建流文件
        /// </summary>
        /// <param name="blocks"></param>
        /// <param name="restart"></param>
        /// <returns></returns>
        internal static RRQMStream GetNewFileStream(ref ProgressBlockCollection blocks, bool restart)
        {
            RRQMStream stream;
            string path = blocks.FileInfo.FilePath + ".rrqm";
            byte[] buffer = new byte[1024 * 1024];

            if (File.Exists(path) && !restart)
            {
                stream = new RRQMStream(path, FileMode.Open, FileAccess.ReadWrite);
                stream.Read(buffer, 0, buffer.Length);
                PBCollectionTemp readBlocks = SerializeConvert.RRQMBinaryDeserialize<PBCollectionTemp>(buffer);
                if (readBlocks.FileInfo.FileHash != null && blocks.FileInfo.FileHash != null && readBlocks.FileInfo.FileHash == blocks.FileInfo.FileHash)
                {
                    blocks = readBlocks.ToPBCollection();
                    stream.fileInfo = blocks.FileInfo;
                    return stream;
                }
                stream.Dispose();
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            byte[] dataBuffer = SerializeConvert.RRQMBinarySerialize(PBCollectionTemp.GetFromProgressBlockCollection(blocks), true);
            for (int i = 0; i < dataBuffer.Length; i++)
            {
                buffer[i] = dataBuffer[i];
            }

            stream = new RRQMStream(path, FileMode.Create, FileAccess.ReadWrite);
            stream.fileInfo = blocks.FileInfo;
            stream.Position = 0;
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            return stream;
        }

        internal static void SaveProgressBlockCollection(RRQMStream stream, ProgressBlockCollection blocks)
        {
            byte[] buffer = new byte[1024 * 1024];
            byte[] dataBuffer = SerializeConvert.RRQMBinarySerialize(PBCollectionTemp.GetFromProgressBlockCollection(blocks), true);
            for (int i = 0; i < dataBuffer.Length; i++)
            {
                buffer[i] = dataBuffer[i];
            }
            stream.Position = 0;
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        internal static bool WriteFile(RRQMStream stream, out string mes, long streamPosition, byte[] buffer, int offset, int length)
        {
            try
            {
                if (stream == null || !stream.CanWrite)
                {
                    mes = "流已释放";
                    return false;
                }
                stream.Position = streamPosition + 1024 * 1024;
                stream.Write(buffer, offset, length);
                stream.Flush();
                mes = null;
                return true;
            }
            catch (Exception ex)
            {
                mes = ex.Message;
                return false;
            }
        }

        internal static void FileFinished(RRQMStream stream)
        {
            stream.Position = 1024 * 1024;
            using (FileStream fileStream = new FileStream(stream.fileInfo.FilePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[1024 * 10];
                while (true)
                {
                    int r = stream.Read(buffer, 0, buffer.Length);
                    if (r == 0)
                    {
                        stream.Dispose();
                        break;
                    }
                    fileStream.Write(buffer, 0, r);
                }
            }
            if (File.Exists(stream.fileInfo.FilePath + ".rrqm"))
            {
                File.Delete(stream.fileInfo.FilePath + ".rrqm");
            }
        }

        internal static ProgressBlockCollection GetProgressBlockCollection(FileInfo fileInfo, bool breakpointResume)
        {
            ProgressBlockCollection blocks = new ProgressBlockCollection();
            blocks.FileInfo = new FileInfo();
            blocks.FileInfo.Copy(fileInfo);
            long position = 0;
            if (breakpointResume && fileInfo.FileLength >= 100)
            {
                long blockLength = (long)(fileInfo.FileLength / 100.0);

                for (int i = 0; i < 100; i++)
                {
                    FileProgressBlock block = new FileProgressBlock();
                    block.Index = i;
                    block.FileHash = fileInfo.FileHash;
                    block.Finished = false;
                    block.StreamPosition = position;
                    block.UnitLength = i != 99 ? blockLength : fileInfo.FileLength - i * blockLength;
                    blocks.Add(block);
                    position += blockLength;
                }
            }
            else
            {
                FileProgressBlock block = new FileProgressBlock();
                block.Index = 0;
                block.FileHash = fileInfo.FileHash;
                block.Finished = false;
                block.StreamPosition = position;
                block.UnitLength = fileInfo.FileLength;
                blocks.Add(block);
            }
            return blocks;
        }

        internal static bool ReadFileBytes(string path, long beginPosition, ByteBlock byteBlock, int offset, int length)
        {
            FileStream fileStream = TransferFileStreamDic.GetFileStream(path);

            fileStream.Position = beginPosition;

            if (byteBlock.Buffer.Length < length + offset)
            {
                byteBlock.SetBuffer(new byte[length + offset]);
            }

            int r = fileStream.Read(byteBlock.Buffer, offset, length);
            if (r == length)
            {
                byteBlock.Position = offset + length;
                return true;
            }
            return false;
        }

        #endregion Methods
    }
}