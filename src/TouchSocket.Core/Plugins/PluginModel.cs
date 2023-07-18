using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    internal class PluginModel
    {
        public List<Func<object, PluginEventArgs, Task>> Funcs = new List<Func<object, PluginEventArgs, Task>>();
        public Method Method;
        public List<IPlugin> Plugins = new List<IPlugin>();
    }
}
