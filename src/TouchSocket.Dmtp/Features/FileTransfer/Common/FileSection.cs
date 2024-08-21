//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件片段句柄。
    /// </summary>
    public class FileSection : PackageBase
    {
        /// <summary>
        /// 文件句柄唯一标识
        /// </summary>
        public int ResourceHandle { get; internal set; }

        /// <summary>
        /// 分块状态。
        /// </summary>
        public FileSectionStatus Status { get; internal set; }

        /// <summary>
        /// 片段的流偏移量。
        /// </summary>
        public long Offset { get; internal set; }

        /// <summary>
        /// 存于集合的索引。
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// 片段长度
        /// </summary>
        public int Length { get; internal set; }

        /// <inheritdoc/>
        public override void Package<TByteBlock>(ref TByteBlock byteBlock)
        {
            byteBlock.WriteByte((byte)this.Status);
            byteBlock.WriteInt32(this.ResourceHandle);
            byteBlock.WriteInt64(this.Offset);
            byteBlock.WriteInt32(this.Length);
            byteBlock.WriteInt32(this.Index);
        }

        /// <inheritdoc/>
        public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
        {
            this.Status = (FileSectionStatus)byteBlock.ReadByte();
            this.ResourceHandle = byteBlock.ReadInt32();
            this.Offset = byteBlock.ReadInt64();
            this.Length = byteBlock.ReadInt32();
            this.Index = byteBlock.ReadInt32();
        }

        /// <summary>
        /// 判断基本信息是否一致。
        /// </summary>
        /// <param name="fileSection">待比较的文件段对象。</param>
        /// <returns>如果待比较的文件段对象的基本信息与当前对象一致，则返回true；否则返回false。</returns>
        public bool Equals(FileSection fileSection)
        {
            // 检查待比较的文件段对象是否为null
            if (fileSection == null)
            {
                // 如果是null，直接返回false，因为无法比较
                return false;
            }
            else
            {
                // 比较两个文件段对象的基本信息（索引、偏移量、长度、资源句柄）是否完全一致
                return this.Index == fileSection.Index &&
                this.Offset == fileSection.Offset &&
                this.Length == fileSection.Length &&
                this.ResourceHandle == fileSection.ResourceHandle;
            }
        }
    }
}