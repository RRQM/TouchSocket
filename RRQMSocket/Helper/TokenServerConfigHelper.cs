using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TokenServer
    /// </summary>
    public static  class TokenServerConfigHelper
    {
        public static TokenServerConfig CreateTokenServerConfig(this ServerConfig serverConfig)
        {
            return new TokenServerConfig();
        }
    }


}
