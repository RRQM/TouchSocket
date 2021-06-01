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
    public class ServerConfig : RRQMDependencyObject
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
        /// 日志记录器
        /// </summary>
        public ILog Logger
        {
            get { return (ILog)GetValue(LoggerProperty); }
            set { SetValue(LoggerProperty, value); }
        }

        /// <summary>
        /// 日志记录器依赖属性
        /// </summary>
        public static readonly DependencyProperty LoggerProperty =
            DependencyProperty.Register("Logger", typeof(ILog), typeof(ServerConfig), new Log());


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


        /// <summary>
        /// 内存池最大尺寸
        /// </summary>
        public long BytePoolMaxSize
        {
            get { return (long)GetValue(BytePoolMaxSizeProperty); }
            set { SetValue(BytePoolMaxSizeProperty, value); }
        }

        /// <summary>
        /// 内存池最大尺寸依赖属性
        /// </summary>
        public static readonly DependencyProperty BytePoolMaxSizeProperty =
            DependencyProperty.Register("BytePoolMaxSize", typeof(long), typeof(ServerConfig), 1024 * 1024 * 512L);


        /// <summary>
        /// 内存池块最大尺寸
        /// </summary>
        public int BytePoolMaxBlockSize
        {
            get { return (int)GetValue(BytePoolMaxBlockSizeProperty); }
            set { SetValue(BytePoolMaxBlockSizeProperty, value); }
        }

        /// <summary>
        /// 内存池块最大尺寸
        /// </summary>
        public static readonly DependencyProperty BytePoolMaxBlockSizeProperty =
            DependencyProperty.Register("BytePoolMaxBlockSize", typeof(int), typeof(ServerConfig), 1024 * 1024 * 20);


        /// <summary>
        /// 缓存池容量
        /// </summary>
        public int BufferLength
        {
            get { return (int)GetValue(BufferLengthProperty); }
            set { SetValue(BufferLengthProperty, value); }
        }

        /// <summary>
        /// 缓存池容量
        /// </summary>
        public static readonly DependencyProperty BufferLengthProperty =
            DependencyProperty.Register("BufferLength", typeof(int), typeof(ServerConfig), 1024);


    }
}
