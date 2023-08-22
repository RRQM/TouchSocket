using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道监听配置
    /// </summary>
    public class NamedPipeListenOption
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 接收、发送缓存大小。同时也是内存池借鉴尺寸。
        /// </summary>
        public int BufferLength { get; set; } = 1024 * 64;

        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// 配置Tcp适配器
        /// </summary>
        public Func<SingleStreamDataHandlingAdapter> Adapter { get; set; } =
            () => new NormalDataHandlingAdapter();
    }
}
