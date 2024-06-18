using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public static class ByteBlockBuilderExtension
    {
        /// <summary>
        /// <inheritdoc cref="IByteBlockBuilder.Build{TByteBlock}(ref TByteBlock)"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="byteBlock"></param>
        public static void Build(this IByteBlockBuilder builder, ByteBlock byteBlock)
        {
            builder.Build(ref byteBlock);
        }
    }
}
