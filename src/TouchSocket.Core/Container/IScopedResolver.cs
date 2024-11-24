using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public interface IScopedResolver:IDisposableObject
    {
        IResolver Resolver { get;}
    }
}
