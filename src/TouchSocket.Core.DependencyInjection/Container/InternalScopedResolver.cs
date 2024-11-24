using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.AspNetCore;

namespace TouchSocket.Core.AspNetCore
{
    internal class InternalScopedResolver : DisposableObject, IScopedResolver
    {
        private readonly IServiceScope m_serviceScope;

        public InternalScopedResolver(IServiceScope serviceScope, IServiceCollection services)
        {
            this.m_serviceScope = serviceScope;
            var scopedResolver=new ScopedResolver(services);
            scopedResolver.ServiceProvider = serviceScope.ServiceProvider;
            this.Resolver = scopedResolver;
        }

        public IResolver Resolver { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_serviceScope.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
