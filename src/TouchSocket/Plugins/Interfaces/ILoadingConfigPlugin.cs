using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 当正在配置Config时触发。
    /// </summary>
    public interface ILoadingConfigPlugin<in TSender>:IPlugin 
    {
        /// <summary>
        /// 当载入配置时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnLoadingConfig(TSender sender, ConfigEventArgs e);
    }

    /// <summary>
    /// ILoadingConfigPlugin
    /// </summary>
    public interface ILoadingConfigPlugin: ILoadingConfigPlugin<object>
    { 
    
    }
}
