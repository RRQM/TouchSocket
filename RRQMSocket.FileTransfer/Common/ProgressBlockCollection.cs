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
    /// <summary>
    /// 文件进度块集合
    /// </summary>
    public class ProgressBlockCollection : ReadOnlyList<FileBlock>
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        public UrlFileInfo UrlFileInfo { get; internal set; }

        private static int blockLength = 1024 * 1024 * 10;

        /// <summary>
        /// 分块长度,min=1024*1024*5
        /// </summary>
        public static int BlockLength
        {
            get { return blockLength; }
            set
            {
                if (value < 1024 * 1024 * 5)
                {
                    value = 1024 * 1024 * 5;
                }
                blockLength = value;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="path"></param>
        internal void Save(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            byte[] buffer = SerializeConvert.RRQMBinarySerialize(this, true);
            using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static ProgressBlockCollection Read(string path)
        {
            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    return SerializeConvert.RRQMBinaryDeserialize<ProgressBlockCollection>(buffer, 0);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static ProgressBlockCollection CreateProgressBlockCollection(UrlFileInfo urlFileInfo)
        {
            ProgressBlockCollection blocks = new ProgressBlockCollection();
            blocks.UrlFileInfo = urlFileInfo;
            long position = 0;
            long surLength = urlFileInfo.FileLength;
            int index = 0;
            while (surLength > 0)
            {
                FileBlock block = new FileBlock();
                block.Index = index++;
                block.RequestStatus = RequestStatus.Hovering;
                block.Position = position;
                block.UnitLength = surLength > blockLength ? blockLength : surLength;
                blocks.Add(block);
                position += block.UnitLength;
                surLength -= block.UnitLength;
            }
            return blocks;
        }
    }
}