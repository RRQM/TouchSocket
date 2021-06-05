using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Dependency;
using RRQMCore.Exceptions;
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfig : RRQMConfig
    {
        /// <summary>
        /// 多线程数量
        /// </summary>
        public int ThreadCount
        {
            get { return (int)GetValue(ThreadCountProperty); }
            set { SetValue(ThreadCountProperty, value); }
        }

        /// <summary>
        /// 多线程数量依赖属性
        /// </summary>
        public static readonly DependencyProperty ThreadCountProperty =
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(ServerConfig), 10);


        /// <summary>
        /// IP和端口号
        /// </summary>
        public IPHost BindIPHost
        {
            get { return (IPHost)GetValue(BindIPHostProperty); }
            set { SetValue(BindIPHostProperty, value); }
        }

        /// <summary>
        /// IP和端口号依赖属性
        /// </summary>
        public static readonly DependencyProperty BindIPHostProperty =
            DependencyProperty.Register("BindIPHost", typeof(IPHost), typeof(ServerConfig), null);
    }
}
