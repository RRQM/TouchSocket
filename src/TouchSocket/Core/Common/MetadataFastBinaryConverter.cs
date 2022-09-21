using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Serialization;

namespace TouchSocket.Core
{
    /// <summary>
    /// MetadataFastBinaryConverter
    /// </summary>
    internal sealed class MetadataFastBinaryConverter : FastBinaryConverter<Metadata>
    {
        protected override Metadata Read(byte[] buffer, int offset, int len)
        {
            ByteBlock byteBlock = new ByteBlock(buffer);
            byteBlock.Pos = offset;
            Metadata metadata = new Metadata();
            while (byteBlock.Pos<offset+len)
            {
                metadata.Add(byteBlock.ReadString(),byteBlock.ReadString());
            }
           return metadata;
        }

        protected override int Write(ByteBlock byteBlock, Metadata obj)
        {
            int pos = byteBlock.Pos;
            foreach (var item in obj.AllKeys)
            {
                byteBlock.Write(item);
                byteBlock.Write(obj[item]);
            }
            return byteBlock.Pos - pos;
        }
    }
}
