using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public static class ByteBlockExtension
    {
        public static ByteBlockStream AsStream(this ByteBlock byteBlock, bool releaseTogether=true)
        {
            return new ByteBlockStream(byteBlock,releaseTogether);
        }

        public static ByteBlock AsByteBlock(this in ValueByteBlock valueByteBlock)
        {
            return new ByteBlock(valueByteBlock);
        }
    }
}
