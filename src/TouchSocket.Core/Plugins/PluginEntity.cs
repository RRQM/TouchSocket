using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    internal class PluginEntity
    {
       public Method Method;
       public IPlugin Plugin;

        public PluginEntity(Method method, IPlugin plugin)
        {
            this.Method = method;
            this.Plugin = plugin;
        }
    }
}
