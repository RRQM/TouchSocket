using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// Tcp服务配置
    /// </summary>
    public class TcpServerConfig : ServerConfig
    {
        /// <summary>
        /// 挂起连接队列的最大长度。默认为30
        /// </summary>
        public int Backlog { get; set; } = 30;

        /// <summary>
        /// 最大可连接数，默认为10000
        /// </summary>
        public int MaxCount { get; set; } = 10000;

        /// <summary>
        /// 检验客户端活性（避免异常而导致的失活），默认为true
        /// </summary>
        public bool IsCheckClientAlive { get; set; } = true;

        /// <summary>
        /// 获取或设置分配ID的格式，
        /// 格式必须符合字符串格式，至少包含一个补位，
        /// 默认为“{0}-TCP”
        /// </summary>
        public string IDFormat { get; set; } = "{0}-TCP";
    }
}
