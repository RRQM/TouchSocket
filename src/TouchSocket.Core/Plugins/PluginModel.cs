using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    internal class PluginModel
    {
        public List<Func<object, PluginEventArgs, Task>> Funcs = new List<Func<object, PluginEventArgs, Task>>();

        //public List<PluginEntity> PluginEntities = new List<PluginEntity>();
    }
}