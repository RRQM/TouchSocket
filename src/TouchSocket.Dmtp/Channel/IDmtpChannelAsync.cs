using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
#if NETCOREAPP3_1_OR_GREATER
    public partial interface IDmtpChannel : IAsyncEnumerable<ByteBlock>
    {

    }
#endif
}
