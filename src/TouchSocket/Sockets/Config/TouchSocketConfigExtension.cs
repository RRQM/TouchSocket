//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Security.Authentication;
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// TouchSocketConfigBuilder配置扩展
    /// </summary>
    public static class TouchSocketConfigExtension
    {
        #region 数据

        /// <summary>
        /// 接收缓存容量，默认1024*64，其作用有两个：
        /// <list type="number">
        /// <item>指示单次可接受的最大数据量</item>
        /// <item>指示常规申请内存块的长度</item>
        /// </list>
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty BufferLengthProperty =
            DependencyProperty.Register("BufferLength", typeof(int), typeof(TouchSocketConfigExtension), 1024 * 64);

        /// <summary>
        /// 数据处理适配器，默认为获取<see cref="NormalDataHandlingAdapter"/>
        /// 所需类型<see cref="Func{TResult}"/>
        /// </summary>
        public static readonly DependencyProperty DataHandlingAdapterProperty =
            DependencyProperty.Register("DataHandlingAdapter", typeof(Func<DataHandlingAdapter>), typeof(TouchSocketConfigExtension),
               (Func<DataHandlingAdapter>)(() => { return new NormalDataHandlingAdapter(); }));

        /// <summary>
        /// 接收类型，默认为<see cref="ReceiveType.Auto"/>
        /// <para><see cref="ReceiveType.Auto"/>为自动接收数据，然后主动触发。</para>
        /// <para><see cref="ReceiveType.None"/>为不投递IO接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。注意：连接端不会感知主动断开</para>
        /// 所需类型<see cref="TouchSocket.Sockets. ReceiveType"/>
        /// </summary>
        public static readonly DependencyProperty ReceiveTypeProperty =
            DependencyProperty.Register("ReceiveType", typeof(ReceiveType), typeof(TouchSocketConfigExtension), ReceiveType.Auto);

        /// <summary>
        /// 数据处理适配器，默认为获取<see cref="UdpDataHandlingAdapter"/>
        /// 所需类型<see cref="Func{TResult}"/>
        /// </summary>
        public static readonly DependencyProperty UdpDataHandlingAdapterProperty =
            DependencyProperty.Register("UdpDataHandlingAdapter", typeof(Func<UdpDataHandlingAdapter>), typeof(TouchSocketConfigExtension),
               (Func<UdpDataHandlingAdapter>)(() => { return new NormalUdpDataHandlingAdapter(); }));

        /// <summary>
        /// 接收缓存容量，默认1024*10，其作用有两个：
        /// <list type="number">
        /// <item>指示单次可接受的最大数据量</item>
        /// <item>指示常规申请内存块的长度</item>
        /// </list>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetBufferLength(this TouchSocketConfig config, int value)
        {
            config.SetValue(BufferLengthProperty, value);
            return config;
        }

        /// <summary>
        /// 设置(Tcp系)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetDataHandlingAdapter(this TouchSocketConfig config, Func<DataHandlingAdapter> value)
        {
            config.SetValue(DataHandlingAdapterProperty, value);
            return config;
        }

        /// <summary>
        /// 接收类型，默认为<see cref="ReceiveType.Auto"/>
        /// <para><see cref="ReceiveType.Auto"/>为自动接收数据，然后主动触发。</para>
        /// <para><see cref="ReceiveType.None"/>为不投递IO接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。注意：连接端不会感知主动断开</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetReceiveType(this TouchSocketConfig config, ReceiveType value)
        {
            config.SetValue(ReceiveTypeProperty, value);
            return config;
        }

        /// <summary>
        /// 设置(Udp系)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetUdpDataHandlingAdapter(this TouchSocketConfig config, Func<UdpDataHandlingAdapter> value)
        {
            config.SetValue(UdpDataHandlingAdapterProperty, value);
            return config;
        }

        #endregion 数据

        #region ServiceBase

        /// <summary>
        /// 服务名称，用于标识，无实际意义，所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ServerNameProperty =
            DependencyProperty.Register("ServerName", typeof(string), typeof(TouchSocketConfigExtension), "RRQMServer");

        /// <summary>
        /// 多线程数量，默认为10。
        /// <para>TCP模式中，该值等效于<see cref="ThreadPool.SetMinThreads(int, int)"/></para>
        /// <para>UDP模式中，该值为重叠IO并发数</para>
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty ThreadCountProperty =
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(TouchSocketConfigExtension), 10);

        /// <summary>
        /// 服务名称，用于标识，无实际意义
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetServerName(this TouchSocketConfig config, string value)
        {
            config.SetValue(ServerNameProperty, value);
            return config;
        }

        /// <summary>
        /// 多线程数量，默认为10。
        /// <para>TCP模式中，该值等效于<see cref="ThreadPool.SetMinThreads(int, int)"/></para>
        /// <para>UDP模式中，该值为重叠IO并发数</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetThreadCount(this TouchSocketConfig config, int value)
        {
            config.SetValue(ThreadCountProperty, value);
            return config;
        }

        #endregion ServiceBase

        #region TcpClient

        /// <summary>
        /// TCP固定端口绑定，
        /// 所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty BindIPHostProperty =
            DependencyProperty.Register("BindIPHost", typeof(IPHost), typeof(TouchSocketConfigExtension), null);

        /// <summary>
        /// 在Socket配置KeepAlive属性，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty KeepAliveValueProperty =
            DependencyProperty.Register("KeepAliveValue", typeof(KeepAliveValue), typeof(TouchSocketConfigExtension), new KeepAliveValue());

        /// <summary>
        /// 数据包最大值。该值会在适当时间，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        public static readonly DependencyProperty MaxPackageSizeProperty =
           DependencyProperty.Register("MaxPackageSize", typeof(int), typeof(TouchSocketConfigExtension), 1024 * 1024 * 10);

        /// <summary>
        /// 设置Socket不使用Delay算法，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty NoDelayProperty =
            DependencyProperty.Register("NoDelay", typeof(bool), typeof(TouchSocketConfigExtension), false);

        /// <summary>
        /// 远程目标地址，所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty RemoteIPHostProperty =
            DependencyProperty.Register("RemoteIPHost", typeof(IPHost), typeof(TouchSocketConfigExtension), null);

        /// <summary>
        /// 在异步发送时，使用独立线程发送，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty SeparateThreadSendProperty =
            DependencyProperty.Register("SeparateThreadSend", typeof(bool), typeof(TouchSocketConfigExtension), false);

        /// <summary>
        /// Ssl配置，为Null时则不启用
        /// 所需类型<see cref="TouchSocket.Sockets.SslOption"/>
        /// </summary>
        public static readonly DependencyProperty SslOptionProperty =
            DependencyProperty.Register("SslOption", typeof(SslOption), typeof(TouchSocketConfigExtension), null);

        /// <summary>
        /// 固定端口绑定。
        /// <para>在<see cref="UdpSessionBase"/>中表示本地监听地址</para>
        /// <para>在<see cref="TcpClientBase"/>中表示固定客户端端口号。</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetBindIPHost(this TouchSocketConfig config, IPHost value)
        {
            config.SetValue(BindIPHostProperty, value);
            return config;
        }

        /// <summary>
        /// 固定端口绑定。
        /// <para>在<see cref="UdpSessionBase"/>中表示本地监听地址</para>
        /// <para>在<see cref="TcpClientBase"/>中表示固定客户端端口号。</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetBindIPHost(this TouchSocketConfig config, int value)
        {
            config.SetValue(BindIPHostProperty, new IPHost(value));
            return config;
        }

        /// <summary>
        /// 固定端口绑定。
        /// <para>在<see cref="UdpSessionBase"/>中表示本地监听地址</para>
        /// <para>在<see cref="TcpClientBase"/>中表示固定客户端端口号。</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetBindIPHost(this TouchSocketConfig config, string value)
        {
            config.SetValue(BindIPHostProperty, new IPHost(value));
            return config;
        }

        /// <summary>
        /// 设置客户端Ssl配置，为Null时则不启用。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetClientSslOption(this TouchSocketConfig config, ClientSslOption value)
        {
            config.SetValue(SslOptionProperty, value);
            return config;
        }

        /// <summary>
        /// 在Socket的KeepAlive属性。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetKeepAliveValue(this TouchSocketConfig config, KeepAliveValue value)
        {
            config.SetValue(KeepAliveValueProperty, value);
            return config;
        }

        /// <summary>
        /// 数据包最大值，默认1024*1024*10。该值会在适当时间，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetMaxPackageSize(this TouchSocketConfig config, int value)
        {
            config.SetValue(MaxPackageSizeProperty, value);
            return config;
        }

        /// <summary>
        /// 设置远程目标地址。在<see cref="UdpSessionBase"/>中，表示默认发送时的目标地址。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetRemoteIPHost(this TouchSocketConfig config, IPHost value)
        {
            config.SetValue(RemoteIPHostProperty, value);
            if (value.IsUri)
            {
                if (value.Uri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase)
                    || value.Uri.Scheme.Equals("wss", StringComparison.CurrentCultureIgnoreCase))
                {
                    config.SetClientSslOption(new ClientSslOption() { TargetHost = value.Host, SslProtocols = SslProtocols.Tls12 });
                }
            }
            return config;
        }

        /// <summary>
        /// 设置远程目标地址。在<see cref="UdpSessionBase"/>中，表示默认发送时的目标地址。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetRemoteIPHost(this TouchSocketConfig config, string value)
        {
            return SetRemoteIPHost(config, new IPHost(value));
        }

        /// <summary>
        /// 设置Socket的NoDelay属性，默认false。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TouchSocketConfig UseNoDelay(this TouchSocketConfig config)
        {
            config.SetValue(NoDelayProperty, true);
            return config;
        }

        /// <summary>
        /// 在异步发送时，使用独立线程发送。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TouchSocketConfig UseSeparateThreadSend(this TouchSocketConfig config)
        {
            config.SetValue(SeparateThreadSendProperty, true);
            return config;
        }

        #endregion TcpClient

        #region TcpService

        /// <summary>
        /// 挂起连接队列的最大长度，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty BacklogProperty =
            DependencyProperty.Register("Backlog", typeof(int), typeof(TouchSocketConfigExtension), 100);

        /// <summary>
        /// 获取或设置清理无数据交互的SocketClient，默认60*1000 ms。如果不想清除，可使用-1。
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty ClearIntervalProperty =
            DependencyProperty.Register("ClearInterval", typeof(int), typeof(TouchSocketConfigExtension), 60 * 1000);

        /// <summary>
        /// 清理统计类型。
        /// <para><see cref="ClearType.Receive"/>为在收到数据时，刷新统计，如果一直有数据接收，则不会被主动清理断开</para>
        /// <para><see cref="ClearType.Send"/>为在发送数据时，刷新统计，如果一直有数据发送，则不会被主动清理断开</para>
        /// <para>二者可叠加使用。</para>
        /// 所需类型<see cref="TouchSocket.Sockets.ClearType"/>
        /// </summary>
        public static readonly DependencyProperty ClearTypeProperty =
            DependencyProperty.Register("ClearType", typeof(ClearType), typeof(TouchSocketConfigExtension), ClearType.Send | ClearType.Receive);

        /// <summary>
        /// 设置默认ID的获取方式，所需类型<see cref="Func{T, TResult}"/>
        /// </summary>
        public static readonly DependencyProperty GetDefaultNewIDProperty =
            DependencyProperty.Register("GetDefaultNewID", typeof(Func<string>), typeof(TouchSocketConfigExtension), null);

        /// <summary>
        /// 服务器负责监听的地址组。所需类型<see cref="IPHost"/>数组
        /// </summary>
        public static readonly DependencyProperty ListenIPHostsProperty =
            DependencyProperty.Register("ListenIPHosts", typeof(IPHost[]), typeof(TouchSocketConfigExtension), null);

        /// <summary>
        /// 最大可连接数，默认为10000，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty MaxCountProperty =
            DependencyProperty.Register("MaxCount", typeof(int), typeof(TouchSocketConfigExtension), 10000);

        /// <summary>
        /// 端口复用，默认为false，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty ReuseAddressProperty =
            DependencyProperty.Register("ReuseAddress", typeof(bool), typeof(TouchSocketConfigExtension), false);

        /// <summary>
        /// 启用端口复用。
        /// <para>该配置可在服务器、或客户端在监听端口时，运行监听同一个端口。可以一定程度缓解端口来不及释放的问题</para>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TouchSocketConfig UseReuseAddress(this TouchSocketConfig config)
        {
            config.SetValue(ReuseAddressProperty, true);
            return config;
        }

        /// <summary>
        /// 挂起连接队列的最大长度，默认100。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetBacklog(this TouchSocketConfig config, int value)
        {
            config.SetValue(BacklogProperty, value);
            return config;
        }

        /// <summary>
        /// 设置清理无数据交互的SocketClient，默认60*1000 ms。如果不想清除，可使用-1
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetClearInterval(this TouchSocketConfig config, int value)
        {
            config.SetValue(ClearIntervalProperty, value);
            return config;
        }

        /// <summary>
        /// 清理统计类型。
        /// <para><see cref="ClearType.Receive"/>为在收到数据时，刷新统计，如果一直有数据接收，则不会被主动清理断开</para>
        /// <para><see cref="ClearType.Send"/>为在发送数据时，刷新统计，如果一直有数据发送，则不会被主动清理断开</para>
        /// <para>二者可叠加使用。</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetClearType(this TouchSocketConfig config, ClearType value)
        {
            config.SetValue(ClearTypeProperty, value);
            return config;
        }

        /// <summary>
        /// 设置默认ID的获取方式。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetGetDefaultNewID(this TouchSocketConfig config, Func<string> value)
        {
            config.SetValue(GetDefaultNewIDProperty, value);
            return config;
        }

        /// <summary>
        /// 服务器负责监听的地址组。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetListenIPHosts(this TouchSocketConfig config, IPHost[] value)
        {
            config.SetValue(ListenIPHostsProperty, value);
            return config;
        }

        /// <summary>
        /// 最大可连接数，默认为10000。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetMaxCount(this TouchSocketConfig config, int value)
        {
            config.SetValue(MaxCountProperty, value);
            return config;
        }

        /// <summary>
        /// 设置客户端Ssl配置，为Null时则不启用。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetServiceSslOption(this TouchSocketConfig config, ServiceSslOption value)
        {
            config.SetValue(SslOptionProperty, value);
            return config;
        }

        #endregion TcpService

        #region UDP

        /// <summary>
        /// 该值指定 System.Net.Sockets.Socket可以发送或接收广播数据包。
        /// </summary>
        public static readonly DependencyProperty EnableBroadcastProperty =
            DependencyProperty.Register("EnableBroadcast", typeof(bool), typeof(TouchSocketConfigExtension), false);

        /// <summary>
        /// 该值指定 System.Net.Sockets.Socket可以发送或接收广播数据包。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TouchSocketConfig UseBroadcast(this TouchSocketConfig config)
        {
            config.SetValue(EnableBroadcastProperty, true);
            return config;
        }

        #endregion UDP

        #region 创建

        /// <summary>
        /// 构建Tcp类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithTcpClient<TClient>(this TouchSocketConfig config) where TClient : ITcpClient
        {
            TClient service = config.Container.Resolve<TClient>();
            service.Setup(config);
            service.Connect();
            return service;
        }

        /// <summary>
        /// 构建Tcp类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpClient BuildWithTcpClient(this TouchSocketConfig config)
        {
            return BuildWithTcpClient<TcpClient>(config);
        }

        /// <summary>
        /// 构建Tcp类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithTcpService<TService>(this TouchSocketConfig config) where TService : ITcpService
        {
            TService service = config.Container.Resolve<TService>();
            service.Setup(config);
            service.Start();
            return service;
        }

        /// <summary>
        /// 构建Tcp类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpService BuildWithTcpService(this TouchSocketConfig config)
        {
            return BuildWithTcpService<TcpService>(config);
        }

        /// <summary>
        /// 构建UDP类，并启动。
        /// </summary>
        /// <typeparam name="TSession"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TSession BuildWithUdpSession<TSession>(this TouchSocketConfig config) where TSession : IUdpSession
        {
            TSession service = config.Container.Resolve<TSession>();
            service.Setup(config);
            service.Start();
            return service;
        }

        /// <summary>
        /// 构建UDP类，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static UdpSession BuildWithUdpSession(this TouchSocketConfig config)
        {
            return BuildWithUdpSession<UdpSession>(config);
        }

        #endregion 创建
    }
}