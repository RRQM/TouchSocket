using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    internal class RedisResponseWaitPackage : WaitPackage
    {

        public byte[] value;

        public override void Package(ByteBlock byteBlock)
        {
            base.Package(byteBlock);
            byteBlock.WriteBytesPackage(value);
        }

        public override void Unpackage(ByteBlock byteBlock)
        {
            base.Unpackage(byteBlock);
            value = byteBlock.ReadBytesPackage();
        }
    }
}
