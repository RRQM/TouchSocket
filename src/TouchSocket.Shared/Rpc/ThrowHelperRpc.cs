using System.Runtime.CompilerServices;
using TouchSocket.Rpc;

namespace TouchSocket.Core
{
    internal static partial class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowRpcException(string message)
        {
            throw new RpcException(message);
        }
    }
}
