using System;

namespace TouchSocket.Core
{
    internal class GuidFastBinaryConverter : FastBinaryConverter<Guid>
    {
        protected override Guid Read(byte[] buffer, int offset, int len)
        {
            var bytes = new byte[16];
            Array.Copy(buffer, offset, bytes, 0, len);

            return new Guid(bytes);
        }

        protected override int Write(ByteBlock byteBlock, Guid obj)
        {
            var bytes = obj.ToByteArray();
            byteBlock.Write(bytes);
            return bytes.Length;
        }
    }
}