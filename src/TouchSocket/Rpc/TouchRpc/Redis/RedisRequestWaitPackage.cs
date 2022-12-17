using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    internal class RedisRequestWaitPackage : RedisResponseWaitPackage
    {
        public string key;
        public TimeSpan? timeSpan;
        public RedisPackageType packageType;

        public override void Package(ByteBlock byteBlock)
        {
            base.Package(byteBlock);
            byteBlock.Write(key);
            byteBlock.Write((byte)packageType);
            if (timeSpan.HasValue)
            {
                byteBlock.Write((byte)1);
                byteBlock.Write(timeSpan.Value);
            }
            else
            {
                byteBlock.Write((byte)0);
            }
        }

        public override void Unpackage(ByteBlock byteBlock)
        {
            base.Unpackage(byteBlock);
            key = byteBlock.ReadString();
            packageType = (RedisPackageType)byteBlock.ReadByte();
            if (byteBlock.ReadByte() == 1)
            {
                timeSpan = byteBlock.ReadTimeSpan();
            }

        }
    }
}
