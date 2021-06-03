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
    /// 配置文件基类
    /// </summary>
    public class RRQMConfig: RRQMDependencyObject
    {
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
            DependencyProperty.Register("Logger", typeof(ILog), typeof(RRQMConfig), new Log());

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
            DependencyProperty.Register("BytePoolMaxSize", typeof(long), typeof(RRQMConfig), 1024 * 1024 * 512L);


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
            DependencyProperty.Register("BytePoolMaxBlockSize", typeof(int), typeof(RRQMConfig), 1024 * 1024 * 20);


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
            DependencyProperty.Register("BufferLength", typeof(int), typeof(RRQMConfig), 1024);
    }
}
