using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcUtility
    /// </summary>
    public static class JsonRpcUtility
    {
        /// <summary>
        /// JsonRpc
        /// </summary>
        public static Protocol JsonRpc { get; private set; } = new Protocol("JsonRpc");

    }
}
