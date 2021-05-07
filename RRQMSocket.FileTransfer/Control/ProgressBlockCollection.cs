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
using System.IO;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件进度块集合
    /// </summary>
    public class ProgressBlockCollection : ReadOnlyList<FileProgressBlock>
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        public FileInfo FileInfo { get; internal set; }

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
    }
}