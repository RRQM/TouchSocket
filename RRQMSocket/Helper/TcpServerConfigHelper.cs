using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
  public static  class TcpServerConfigHelper
    {
        public static TcpServerConfig CreateTcpServerConfig(this ServerConfig serverConfig)
        {
            return new TcpServerConfig();
        }
    }


}
