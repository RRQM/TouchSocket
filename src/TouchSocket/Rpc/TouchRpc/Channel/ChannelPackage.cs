using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
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
        public ArraySegment<byte> Data { get; set; }
        public ChannelDataType DataType { get; set; }
        public bool RunNow { get; set; }
        int IQueueData.Size => this.Data == null ? 0 : this.Data.Count;

        public int GetLen()
        {
            if (Data == null)
            {
                return 1024;
            }
            return Data.Count + 1024;
        }

        public override void PackageBody(ByteBlock byteBlock)
        {
            byteBlock.Write(RunNow);
            byteBlock.Write((byte)DataType);
            byteBlock.Write(ChannelId);
            byteBlock.WriteBytesPackage(Data.Array, Data.Offset, Data.Count);
        }

        public override void UnpackageBody(ByteBlock byteBlock)
        {
            this.RunNow = byteBlock.ReadBoolean();
            this.DataType = (ChannelDataType)byteBlock.ReadByte();
            this.ChannelId = byteBlock.ReadInt32();
            var data = byteBlock.ReadBytesPackage();
            if (data != null)
            {
                this.Data = new ArraySegment<byte>(data);
            }
        }
    }
}