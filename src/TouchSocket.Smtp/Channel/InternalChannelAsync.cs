using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Smtp
{
#if NETCOREAPP3_1_OR_GREATER
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
