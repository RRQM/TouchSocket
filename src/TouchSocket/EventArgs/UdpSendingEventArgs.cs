using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    public class UdpSendingEventArgs : SendingEventArgs
    {
        public UdpSendingEventArgs(in ReadOnlyMemory<byte> memory,EndPoint endPoint) : base(memory)
        {
            this.EndPoint = endPoint;
        }

        public EndPoint EndPoint { get; }
    }
}
