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
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    internal class WaitSmallFilePackage : WaitRouterPackage
    {
        public byte[] Data { get; set; }
        public RemoteFileInfo FileInfo { get; set; }
        public int Len { get; set; }
        public Metadata Metadata { get; set; }
        public string Path { get; set; }

        public override void PackageBody(ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.Write(this.Path);
            byteBlock.WritePackage(this.Metadata);
            byteBlock.WritePackage(this.FileInfo);
            byteBlock.WriteBytesPackage(this.Data, 0, this.Len);
        }

        public override void UnpackageBody(ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.Path = byteBlock.ReadString();
            this.Metadata = byteBlock.ReadPackage<Metadata>();
            this.FileInfo = byteBlock.ReadPackage<RemoteFileInfo>();
            this.Data = byteBlock.ReadBytesPackage();
        }
    }
}