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
using RRQMCore.Dependency;

namespace RRQMSocket
{
    /// <summary>
    /// TcpClient配置
    /// </summary>
    public class TcpClientConfig : ClientConfig
    {
        /// <summary>
        /// Ssl配置，为Null时则不启用
        /// </summary>
        public ClientSslOption SslOption
        {
            get => (ClientSslOption)this.GetValue(SslOptionProperty);
            set => this.SetValue(SslOptionProperty, value);
        }

        /// <summary>
        /// Ssl配置，为Null时则不启用
        /// 所需类型<see cref="RRQMSocket.ClientSslOption"/>
        /// </summary>
        public static readonly DependencyProperty SslOptionProperty =
            DependencyProperty.Register("SslOption", typeof(ClientSslOption), typeof(TcpClientConfig), null);

        /// <summary>
        /// 远程IPHost
        /// </summary>
        public IPHost RemoteIPHost
        {
            get => (IPHost)this.GetValue(RemoteIPHostProperty);
            set => this.SetValue(RemoteIPHostProperty, value);
        }

        /// <summary>
        /// 远程IPHost，所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty RemoteIPHostProperty =
            DependencyProperty.Register("RemoteIPHost", typeof(IPHost), typeof(TcpClientConfig), null);

        /// <summary>
        /// 仅发送，即不开启接收线程，
        /// 同时不会感知断开操作。
        /// </summary>
        public bool OnlySend
        {
            get => (bool)this.GetValue(OnlySendProperty);
            set => this.SetValue(OnlySendProperty, value);
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
            get => (bool)this.GetValue(SeparateThreadSendProperty);
            set => this.SetValue(SeparateThreadSendProperty, value);
        }

        /// <summary>
        /// 在异步发送时，使用独立线程发送，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty SeparateThreadSendProperty =
            DependencyProperty.Register("SeparateThreadSend", typeof(bool), typeof(TcpClientConfig), false);

        /// <summary>
        /// 接收类型
        /// </summary>
        public ReceiveType ReceiveType
        {
            get => (ReceiveType)this.GetValue(ReceiveTypeProperty);
            set => this.SetValue(ReceiveTypeProperty, value);
        }

        /// <summary>
        /// 接收类型，所需类型<see cref="RRQMSocket. ReceiveType"/>
        /// </summary>
        public static readonly DependencyProperty ReceiveTypeProperty =
            DependencyProperty.Register("ReceiveType", typeof(ReceiveType), typeof(TcpClientConfig), ReceiveType.IOCP);

        /// <summary>
        /// 在Socket配置KeepAlive属性
        /// </summary>
        public bool KeepAlive
        {
            get => (bool)this.GetValue(KeepAliveProperty);
            set => this.SetValue(KeepAliveProperty, value);
        }

        /// <summary>
        /// 在Socket配置KeepAlive属性，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty KeepAliveProperty =
            DependencyProperty.Register("KeepAlive", typeof(bool), typeof(TcpClientConfig), false);

        /// <summary>
        /// 设置Socket不使用Delay算法
        /// </summary>
        public bool NoDelay
        {
            get => (bool)this.GetValue(NoDelayProperty);
            set => this.SetValue(NoDelayProperty, value);
        }

        /// <summary>
        /// 设置Socket不使用Delay算法，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty NoDelayProperty =
            DependencyProperty.Register("NoDelay", typeof(bool), typeof(TcpClientConfig), false);

        /// <summary>
        /// TCP固定端口绑定
        /// </summary>
        public IPHost BindIPHost
        {
            get => (IPHost)this.GetValue(BindIPHostProperty);
            set => this.SetValue(BindIPHostProperty, value);
        }

        /// <summary>
        /// TCP固定端口绑定，
        /// 所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty BindIPHostProperty =
            DependencyProperty.Register("BindIPHost", typeof(IPHost), typeof(TcpClientConfig), null);
    }
}