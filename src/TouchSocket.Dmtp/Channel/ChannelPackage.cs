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

namespace TouchSocket.Dmtp
{
    internal enum ChannelDataType : byte
    {
        DataOrder,
        CompleteOrder,
        CancelOrder,
        DisposeOrder,
        HoldOnOrder,
        QueueRun,
        QueuePause
    }

    internal class ChannelPackage : MsgRouterPackage, IQueueData
    {
        public int ChannelId { get; set; }
        public ByteBlock Data { get; set; }
        public ChannelDataType DataType { get; set; }
        public bool RunNow { get; set; }
        int IQueueData.Size => this.Data == null ? 0 : this.Data.Len;

        public int GetLen()
        {
            return this.Data == null ? 1024 : this.Data.Len + 1024;
        }

        public override void PackageBody(ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.Write(this.RunNow);
            byteBlock.Write((byte)this.DataType);
            byteBlock.Write(this.ChannelId);
            
            byteBlock.WriteByteBlock(this.Data);
        }

        public override void UnpackageBody(ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.RunNow = byteBlock.ReadBoolean();
            this.DataType = (ChannelDataType)byteBlock.ReadByte();
            this.ChannelId = byteBlock.ReadInt32();
            this.Data = byteBlock.ReadByteBlock();
        }
    }
}