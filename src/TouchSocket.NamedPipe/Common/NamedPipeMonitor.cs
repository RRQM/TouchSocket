using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道监听器
    /// </summary>
    public class NamedPipeMonitor
    {
        /// <summary>
        /// 命名管道监听器
        /// </summary>
        /// <param name="option"></param>
        public NamedPipeMonitor(NamedPipeListenOption option)
        {
            this.Option = option;
        }

        /// <summary>
        /// 命名管道监听配置
        /// </summary>
        public NamedPipeListenOption Option { get; }
    }
}
