using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    internal class ProxyModel
    {
        public MethodInstance MethodInstance { get; set; }
        public string InvokeKey { get; set; }
        public bool InvokeOption { get; set; }
        public Method GenericMethod { get; set; }
    }
}
