namespace TouchSocket.Core
{
    /// <summary>
    /// PluginBase
    /// </summary>
    public class PluginBase : DisposableObject, IPlugin
    {
        /// <inheritdoc/>
        public int Order { get; set; }

        /// <inheritdoc cref="IPlugin.Loaded(IPluginsManager)"/>
        protected virtual void Loaded(IPluginsManager pluginsManager)
        {

        }

        void IPlugin.Loaded(IPluginsManager pluginsManager)
        {
            this.Loaded(pluginsManager);
        }
    }
}
