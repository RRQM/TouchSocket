//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
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
        /// 内部日志记录器，可自行继承<see cref="ILog"/>接口实现，默认为控制台输出。
        /// </summary>
        public ILog Logger
        {
            get { return (ILog)this.GetValue(LoggerProperty); }
            set { this.SetValue(LoggerProperty, value); }
        }

        /// <summary>
        /// 内部日志记录器，可自行继承<see cref="ILog"/>接口实现，默认为控制台输出。所需类型<see cref="ILog"/>
        /// </summary>
        public static readonly DependencyProperty LoggerProperty =
            DependencyProperty.Register("Logger", typeof(ILog), typeof(RRQMConfig), new ConsoleLogger());

        /// <summary>
        /// 接收缓存容量，默认1024*10，其作用有两个：
        /// <list type="number">
        /// <item>指示单次可接受的最大数据量</item>
        /// <item>指示常规申请内存块的长度</item>
        /// </list>
        /// </summary>
        public int BufferLength
        {
            get { return (int)this.GetValue(BufferLengthProperty); }
            set { this.SetValue(BufferLengthProperty, value); }
        }

        /// <summary>
        /// 接收缓存容量，默认1024*10，其作用有两个：
        /// <list type="number">
        /// <item>指示单次可接受的最大数据量</item>
        /// <item>指示常规申请内存块的长度</item>
        /// </list>
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty BufferLengthProperty =
            DependencyProperty.Register("BufferLength", typeof(int), typeof(RRQMConfig), 1024 * 10);

        /// <summary>
        /// 接收类型，默认为<see cref="ReceiveType.IOCP"/>
        /// <para><see cref="ReceiveType.IOCP"/>为完成端口IO模式，在一般情况下该模式效率最高，但是不支持Ssl。</para>
        /// <para><see cref="ReceiveType.BIO"/>为线程阻塞模式，意味着每个接收对应一个线程。</para>
        /// <para><see cref="ReceiveType.Select"/>为选择IO模式，可容纳10w连接。</para>
        /// <para><see cref="ReceiveType.None"/>为不投递IO接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。注意：连接端不会感知主动断开</para>
        /// </summary>
        public ReceiveType ReceiveType
        {
            get { return (ReceiveType)this.GetValue(ReceiveTypeProperty); }
            set { this.SetValue(ReceiveTypeProperty, value); }
        }

        /// <summary>
        /// 接收类型，默认为<see cref="ReceiveType.IOCP"/>
        /// <para><see cref="ReceiveType.IOCP"/>为完成端口IO模式，在一般情况下该模式效率最高，但是不支持Ssl。</para>
        /// <para><see cref="ReceiveType.BIO"/>为线程阻塞模式，意味着每个接收对应一个线程。</para>
        /// <para><see cref="ReceiveType.Select"/>为选择IO模式，可容纳10w连接。</para>
        /// <para><see cref="ReceiveType.None"/>为不投递IO接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。注意：连接端不会感知主动断开</para>
        /// 所需类型<see cref="RRQMSocket. ReceiveType"/>
        /// </summary>
        public static readonly DependencyProperty ReceiveTypeProperty =
            DependencyProperty.Register("ReceiveType", typeof(ReceiveType), typeof(ServiceConfig), ReceiveType.IOCP);
    }
}