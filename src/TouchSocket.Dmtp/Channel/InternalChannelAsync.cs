using System.Collections.Generic;
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
#if NET6_0_OR_GREATER
    internal partial class InternalChannel
    {
        public async IAsyncEnumerator<ByteBlock> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            while (await this.MoveNextAsync())
            {
                yield return this.GetCurrent();
            }
        }
    }
#endif
}