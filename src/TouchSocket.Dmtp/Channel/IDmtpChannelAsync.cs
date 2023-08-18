using System.Collections.Generic;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
#if NETCOREAPP3_1_OR_GREATER
    public partial interface IDmtpChannel : IAsyncEnumerable<ByteBlock>
    {

    }
#endif
}
