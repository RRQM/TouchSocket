namespace TouchSocket.Core
{
    /// <summary>
    /// 消息包
    /// </summary>
    public class MsgPackage : PackageBase
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <inheritdoc/>
        public override void Package(in ByteBlock byteBlock)
        {
            byteBlock.Write(this.Message);
        }

        /// <inheritdoc/>
        public override void Unpackage(in ByteBlock byteBlock)
        {
            this.Message = byteBlock.ReadString();
        }
    }
}