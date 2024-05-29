using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Core
{
    internal static partial class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowClientNotConnectedException()
        {
            throw new ClientNotConnectedException(TouchSocketResource.ClientNotConnected);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowClientNotFindException(string id)
        {
            throw new ClientNotFindException(TouchSocketResource.ClientNotFind.Format(id));
        }
    }
}
