//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    internal class WaitFileSection : WaitRouterPackage
    {
        public FileSection FileSection { get; set; }
        public ArraySegment<byte> Value { get; set; }

        public override void PackageBody(ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.WritePackage(this.FileSection);
            if (this.Value.Array == null)
            {
                byteBlock.WriteNull();
            }
            else
            {
                byteBlock.WriteNotNull();
                byteBlock.WriteBytesPackage(this.Value.Array, this.Value.Offset, this.Value.Count);
            }
        }

        public override void UnpackageBody(ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.FileSection = byteBlock.ReadPackage<FileSection>();
            var isNull = byteBlock.ReadIsNull();
            if (!isNull)
            {
                var bytes = byteBlock.ReadBytesPackage();
                this.Value = new ArraySegment<byte>(bytes);
            }
        }
    }
}