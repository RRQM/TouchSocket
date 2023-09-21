using System;
using System.Data;

namespace TouchSocket.Core
{
    internal class DataTableFastBinaryConverter : FastBinaryConverter<DataTable>
    {
        protected override DataTable Read(byte[] buffer, int offset, int len)
        {
            var bytes = new byte[len];
            Array.Copy(buffer, offset, bytes, 0, len);
            return SerializeConvert.BinaryDeserialize<DataTable>(bytes);
        }

        protected override int Write(ByteBlock byteBlock, DataTable obj)
        {
            var bytes = SerializeConvert.BinarySerialize(obj);
            byteBlock.Write(bytes);
            return bytes.Length;
        }
    }
}