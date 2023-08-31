using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    class DataSetFastBinaryConverter : FastBinaryConverter<DataSet>
    {
        protected override DataSet Read(byte[] buffer, int offset, int len)
        {
            var bytes = new byte[len];
            Array.Copy(buffer, offset, bytes, 0, len);
            return SerializeConvert.BinaryDeserialize<DataSet>(bytes);
        }

        protected override int Write(ByteBlock byteBlock, DataSet obj)
        {
            var bytes = SerializeConvert.BinarySerialize(obj);
            byteBlock.Write(bytes);
            return bytes.Length;
        }
    }
}
