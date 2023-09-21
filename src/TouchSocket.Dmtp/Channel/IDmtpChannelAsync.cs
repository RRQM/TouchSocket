using System.Collections.Generic;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
#if NET6_0_OR_GREATER
    public partial interface IDmtpChannel : IAsyncEnumerable<ByteBlock>
    {
    }
#endif
}