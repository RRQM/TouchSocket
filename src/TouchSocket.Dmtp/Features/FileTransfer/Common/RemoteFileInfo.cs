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

using System.IO;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// RemoteFileInfo
    /// </summary>
    public class RemoteFileInfo : RemoteFileSystemInfo
    {
        /// <summary>
        /// 初始化一个RemoteFileInfo
        /// </summary>
        public RemoteFileInfo()
        {
        }

        /// <summary>
        /// 从FileInfo初始化一个RemoteFileInfo
        /// </summary>
        /// <param name="fileInfo"></param>
        public RemoteFileInfo(FileInfo fileInfo)
        {
            this.FullName = fileInfo.FullName;
            this.Name = fileInfo.Name;
            this.Length = fileInfo.Length;
            this.Attributes = fileInfo.Attributes;
            this.CreationTime = fileInfo.CreationTime;
            this.LastWriteTime = fileInfo.LastWriteTime;
            this.LastAccessTime = fileInfo.LastAccessTime;
        }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 文件MD5
        /// </summary>
        public string MD5 { get; set; }

        /// <inheritdoc/>
        public override void Package<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.Package(ref byteBlock);
            byteBlock.WriteString(this.MD5);
            byteBlock.WriteInt64(this.Length);
        }

        /// <inheritdoc/>
        public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.Unpackage(ref byteBlock);
            this.MD5 = byteBlock.ReadString();
            this.Length = byteBlock.ReadInt64();
        }
    }
}