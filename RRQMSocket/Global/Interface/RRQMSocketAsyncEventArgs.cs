using RRQMCore.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    class RRQMSocketAsyncEventArgs : SocketAsyncEventArgs, IPoolObject
    {
        public bool NewCreat { get; set; }

        public void Create()
        {
            
        }

        public void Destroy()
        {
           
        }

        public void Recreate()
        {
            
        }
    }
}
