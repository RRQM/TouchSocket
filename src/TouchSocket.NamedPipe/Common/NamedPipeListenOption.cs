using System;
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
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// 配置适配器
        /// </summary>
        public Func<SingleStreamDataHandlingAdapter> Adapter { get; set; } =
            () => new NormalDataHandlingAdapter();
    }
}