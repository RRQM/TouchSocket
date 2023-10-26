namespace TouchSocket.Core
{
    internal class StringFastBinaryConverter : FastBinaryConverter<string>
    {
        protected override string Read(byte[] buffer, int offset, int len)
        {
            var byteBlock = new ValueByteBlock(buffer)
            {
                Pos = offset
            };
            return byteBlock.ReadString();
        }

        protected override int Write(ByteBlock byteBlock, string obj)
        {
            var pos = byteBlock.Pos;
            byteBlock.Write(obj);
            return byteBlock.Pos - pos;
        }
    }
}