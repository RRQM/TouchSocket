using System;

namespace TouchSocket.Core
{
    internal class VersionFastBinaryConverter : FastBinaryConverter<Version>
    {
        protected override Version Read(byte[] buffer, int offset, int len)
        {
            var byteBlock = new ValueByteBlock(buffer);
            byteBlock.Pos = offset;
            return new Version(byteBlock.ReadInt32(), byteBlock.ReadInt32(), byteBlock.ReadInt32(), byteBlock.ReadInt32());
        }

        protected override int Write(ByteBlock byteBlock, Version obj)
        {
            byteBlock.Write(obj.Major);
            byteBlock.Write(obj.Minor);
            byteBlock.Write(obj.Build);
            byteBlock.Write(obj.Revision);
            return sizeof(int) * 4;
        }
    }
}
