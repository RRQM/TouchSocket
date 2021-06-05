using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 异步字节
    /// </summary>
    internal struct AsyncByte
    {
        internal int offset;
        internal int length;
        internal byte[] buffer;
    }
}
