using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public interface IServerConfig
    {
        /// <summary>
        /// 多线程数量
        /// </summary>
        int ThreadCount { get; set; }

        /// <summary>
        /// IP和端口号
        /// </summary>
        IPHost IPHost { get; set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; set; }
    }
}
