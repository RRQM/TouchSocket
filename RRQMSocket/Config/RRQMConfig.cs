//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Dependency;
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// 配置文件基类
    /// </summary>
    public class RRQMConfig : RRQMDependencyObject
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
        /// 日志记录器依赖属性，所需类型<see cref="ILog"/>
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
        /// 内存池最大尺寸依赖属性，所需类型<see cref="long"/>
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
        /// 内存池块最大尺寸，所需类型<see cref="int"/>
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
        /// 缓存池容量，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty BufferLengthProperty =
            DependencyProperty.Register("BufferLength", typeof(int), typeof(RRQMConfig), 1024);
    }
}