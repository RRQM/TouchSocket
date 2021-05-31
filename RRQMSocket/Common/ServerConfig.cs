using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Dependency;
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfig :RRQMDependencyObject, IServerConfig
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ServerConfig()
        {
            this.ThreadCount = 10;
            this.Logger = new Log();
            this.BytePoolMaxSize = 1024 * 1024 * 512;
            this.BytePoolMaxBlockSize = 1024 * 1024 * 20;
        }
        /// <summary>
        /// 多线程数量
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// IP和端口号
        /// </summary>
        public IPHost IPHost { get; set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// 内存池最大尺寸
        /// </summary>
        public long BytePoolMaxSize { get; set; }

        /// <summary>
        /// 内存池块最大尺寸
        /// </summary>
        public int BytePoolMaxBlockSize { get; set; }



        /// <summary>
        /// 构建配置
        /// </summary>
        /// <returns></returns>
        public virtual IServerConfig Build()
        {
            return this;
        }
    }
}
