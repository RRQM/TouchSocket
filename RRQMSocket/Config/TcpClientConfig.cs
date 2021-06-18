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
using RRQMCore.ByteManager;
using RRQMCore.Dependency;

namespace RRQMSocket
{
    /// <summary>
    /// TcpClient配置
    /// </summary>
    public class TcpClientConfig : ClientConfig
    {
        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get { return (DataHandlingAdapter)GetValue(DataHandlingAdapterProperty); }
            set { SetValue(DataHandlingAdapterProperty, value); }
        }

        /// <summary>
        /// 数据处理适配器，所需类型<see cref="RRQMSocket.DataHandlingAdapter"/>
        /// </summary>
        public static readonly DependencyProperty DataHandlingAdapterProperty =
            DependencyProperty.Register("DataHandlingAdapter", typeof(DataHandlingAdapter), typeof(TcpClientConfig), new NormalDataHandlingAdapter());

        /// <summary>
        /// 远程IPHost
        /// </summary>
        public IPHost RemoteIPHost
        {
            get { return (IPHost)GetValue(RemoteIPHostProperty); }
            set { SetValue(RemoteIPHostProperty, value); }
        }

        /// <summary>
        /// 远程IPHost，所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty RemoteIPHostProperty =
            DependencyProperty.Register("RemoteIPHost", typeof(IPHost), typeof(TcpClientConfig), null);

        /// <summary>
        /// 内存池实例
        /// </summary>
        public BytePool BytePool
        {
            get { return (BytePool)GetValue(BytePoolProperty); }
            set { SetValue(BytePoolProperty, value); }
        }

        /// <summary>
        /// 内存池实例，所需类型<see cref="RRQMCore.ByteManager.BytePool"/>
        /// </summary>
        public static readonly DependencyProperty BytePoolProperty =
            DependencyProperty.Register("BytePool", typeof(BytePool), typeof(TcpClientConfig), new BytePool());

        /// <summary>
        /// 仅发送，即不开启接收线程，
        /// 同时不会感知断开操作。
        /// </summary>
        public bool OnlySend
        {
            get { return (bool)GetValue(OnlySendProperty); }
            set { SetValue(OnlySendProperty, value); }
        }

        /// <summary>
        /// 仅发送，即不开启接收线程，
        /// 同时不会感知断开操作，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty OnlySendProperty =
            DependencyProperty.Register("OnlySend", typeof(bool), typeof(TcpClientConfig), false);

        /// <summary>
        /// 在异步发送时，使用独立线程发送
        /// </summary>
        public bool SeparateThreadSend
        {
            get { return (bool)GetValue(SeparateThreadSendProperty); }
            set { SetValue(SeparateThreadSendProperty, value); }
        }

        /// <summary>
        /// 在异步发送时，使用独立线程发送，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty SeparateThreadSendProperty =
            DependencyProperty.Register("SeparateThreadSend", typeof(bool), typeof(TcpClientConfig), false);

        /// <summary>
        /// 独立线程发送缓存区
        /// </summary>
        public int SeparateThreadSendBufferLength
        {
            get { return (int)GetValue(SeparateThreadSendBufferLengthProperty); }
            set { SetValue(SeparateThreadSendBufferLengthProperty, value); }
        }

        /// <summary>
        /// 独立线程发送缓存区，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty SeparateThreadSendBufferLengthProperty =
            DependencyProperty.Register("SeparateThreadSendBufferLength", typeof(int), typeof(TcpClientConfig), 1024);
    }
}