using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// DateHandleAdapterExtension
    /// </summary>
    public static class DataHandlingAdapterExtension
    {
        #region Tcp

        /// <summary>
        /// 将<see cref="TouchSocketConfig"/>中的配置，装载在<see cref="TcpDataHandlingAdapter"/>上。
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="config"></param>
        public static void Config(this TcpDataHandlingAdapter adapter, TouchSocketConfig config)
        {
            if (config.GetValue(TouchSocketConfigExtension.MaxPackageSizeProperty) is int v1)
            {
                adapter.MaxPackageSize = v1;
            }
            if (config.GetValue(TouchSocketConfigExtension.CacheTimeoutProperty) != TimeSpan.Zero)
            {
                adapter.CacheTimeout = config.GetValue(TouchSocketConfigExtension.CacheTimeoutProperty);
            }
            if (config.GetValue(TouchSocketConfigExtension.CacheTimeoutEnableProperty) is bool v2)
            {
                adapter.CacheTimeoutEnable = v2;
            }
            if (config.GetValue(TouchSocketConfigExtension.UpdateCacheTimeWhenRevProperty) is bool v3)
            {
                adapter.UpdateCacheTimeWhenRev = v3;
            }
        }
        #endregion
    }
}
