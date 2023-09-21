using System.IO;

namespace TouchSocket.Core
{
    internal class MemoryStreamFastBinaryConverter : FastBinaryConverter<MemoryStream>
    {
        protected override MemoryStream Read(byte[] buffer, int offset, int len)
        {
            var memoryStream = new MemoryStream(len);
            memoryStream.Write(buffer, offset, len);
            memoryStream.Position = 0;
            return memoryStream;
        }

        protected override int Write(ByteBlock byteBlock, MemoryStream obj)
        {
#if !NET45
            if (obj.TryGetBuffer(out var array))
            {
                byteBlock.Write(array.Array,array.Offset,array.Count);
                return array.Count;
            }
#endif
            var bytes = obj.ToArray();
            byteBlock.Write(bytes);
            return bytes.Length;
        }
    }
}