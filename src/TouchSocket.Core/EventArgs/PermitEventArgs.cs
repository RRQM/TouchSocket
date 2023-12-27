using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有操作许可的事件参数类
    /// </summary>
    public class PermitEventArgs:PluginEventArgs
    {
        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool IsPermitOperation { get; set; }
    }
}
