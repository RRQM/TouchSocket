using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// WaitCreateChannel
    /// </summary>
    class WaitCreateChannelPackage : WaitRouterPackage
    {
        /// <summary>
        /// 随机ID
        /// </summary>
        public bool Random{ get; set; }

        /// <summary>
        /// 通道ID
        /// </summary>
        public int ChannelID { get; set; }

        public override void PackageBody(ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.Write(Random);
            byteBlock.Write(ChannelID);
        }

        public override void UnpackageBody(ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.Random=byteBlock.ReadBoolean();
            this.ChannelID=byteBlock.ReadInt32();
        }
    }
}
