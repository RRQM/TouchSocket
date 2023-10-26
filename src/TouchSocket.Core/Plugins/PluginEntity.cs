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

        public Task Run(object sender, PluginEventArgs e)
        {
            return this.Method.InvokeAsync(this.Plugin, sender, e);
        }
    }
}