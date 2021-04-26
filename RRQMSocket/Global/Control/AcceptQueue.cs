using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    internal class AcceptQueue:IDisposable
    {
        internal AcceptQueue()
        {
            this.waitHandle = new AutoResetEvent(false);
            this.sockets = new System.Collections.Concurrent.ConcurrentQueue<Socket>();
        }
        internal Thread Thread;
        internal EventWaitHandle waitHandle;
        internal System.Collections.Concurrent.ConcurrentQueue<Socket> sockets;

        public void Dispose()
        {
            this.waitHandle.Dispose();
            while (this.sockets.TryDequeue(out _))
            {

            }
        }
    }
}
