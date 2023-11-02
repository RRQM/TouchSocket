using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有配置设置的对象
    /// </summary>
    public abstract class ConfigObject : DependencyObject, IConfigObject
    {
        /// <inheritdoc/>
        public abstract TouchSocketConfig Config { get;}

        /// <inheritdoc/>
        public ILog Logger { get; set; }
    }
}