namespace TouchSocket.Core
{
    internal class ByteBlockFastBinaryConverter : FastBinaryConverter<ByteBlock>
    {
        protected override ByteBlock Read(byte[] buffer, int offset, int len)
        {
            var byteBlock = new ByteBlock(len);
            byteBlock.Write(buffer, offset, len);
            byteBlock.SeekToStart();
            return byteBlock;
        }

        protected override int Write(ByteBlock byteBlock, ByteBlock obj)
        {
            byteBlock.Write(obj.Buffer, 0, obj.Len);
            return obj.Len;
        }
    }
}