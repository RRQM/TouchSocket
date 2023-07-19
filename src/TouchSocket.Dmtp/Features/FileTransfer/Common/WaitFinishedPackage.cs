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
    internal class WaitFinishedPackage : WaitRouterPackage
    {
        public WaitFinishedPackage()
        {
            this.UnFinishedIndexs = new int[0];
        }

        public Metadata Metadata { get; set; }
        public int ResourceHandle { get; set; }
        public int[] UnFinishedIndexs { get; set; }

        public override void PackageBody(ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.Write(this.ResourceHandle);
            byteBlock.WritePackage(this.Metadata);
            byteBlock.Write(this.UnFinishedIndexs.Length);
            foreach (var item in this.UnFinishedIndexs)
            {
                byteBlock.Write(item);
            }
        }

        public override void UnpackageBody(ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.ResourceHandle = byteBlock.ReadInt32();
            this.Metadata = byteBlock.ReadPackage<Metadata>();
            var len = byteBlock.ReadInt32();
            if (len == 0)
            {
                return;
            }

            this.UnFinishedIndexs = new int[len];
            for (var i = 0; i < len; i++)
            {
                this.UnFinishedIndexs[i] = byteBlock.ReadInt32();
            }
        }
    }
}