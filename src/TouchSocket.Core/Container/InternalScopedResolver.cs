using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    internal class InternalScopedResolver : DisposableObject, IScopedResolver
    {
        public IResolver Resolver { get; set; }

        public InternalScopedResolver(IResolver resolver)
        {
            this.Resolver = resolver;
        }
    }
    
}
