using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfig : IServerConfig
    {
        /// <summary>
        /// 多线程数量
        /// </summary>
        public int ThreadCount { get; set; } = 1;

        /// <summary>
        /// IP和端口号
        /// </summary>
        public IPHost IPHost { get; set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; set; } = new Log();
    }
}
