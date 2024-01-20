using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    internal class CorsService : ICorsService
    {
        private readonly Dictionary<string, CorsPolicy> m_corsPolicys;

        public CorsService(CorsOptions corsOptions)
        {
            m_corsPolicys = corsOptions.CorsPolicys;
        }

        public CorsPolicy GetPolicy(string name)
        {
            if (m_corsPolicys.TryGetValue(name,out var corsPolicy))
            {
                return corsPolicy;
            }

            return null;
        }
    }
}
