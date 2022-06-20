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
using RRQMCore.Log;
using System;
using System.Security.Authentication;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// RRQMConfig配置扩展
    /// </summary>
    public static class RRQMConfigExtensions
    {
        #region NAT

        /// <summary>
        /// 转发的类型，
        /// 所需类型<see cref="RRQMSocket.NATMode"/>
        /// </summary>
        public static readonly DependencyProperty NATModeProperty =
            DependencyProperty.Register("NATMode", typeof(NATMode), typeof(RRQMConfigExtensions), NATMode.TwoWay);

        /// <summary>
        /// 转发的目标地址集合，
        /// 所需类型<see cref="IPHost"/>数组
        /// </summary>
        public static readonly DependencyProperty TargetIPHostsProperty =
            DependencyProperty.Register("TargetIPHosts", typeof(IPHost[]), typeof(RRQMConfigExtensions), null);

        /// <summary>
        ///  转发的类型
        ///  <para>仅适用于<see cref="NATService"/>及派生类</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetNATMode(this RRQMConfig config, NATMode value)
        {
            config.SetValue(NATModeProperty, value);
            return config;
        }

        /// <summary>
        /// 转发的目标地址集合。
        /// <para>仅适用于<see cref="NATService"/>及派生类</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetTargetIPHosts(this RRQMConfig config, IPHost[] value)
        {
            config.SetValue(TargetIPHostsProperty, value);
            return config;
        }

        #endregion NAT

        #region ServiceBase

        /// <summary>
        /// 服务名称，用于标识，无实际意义，所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ServerNameProperty =
            DependencyProperty.Register("ServerName", typeof(string), typeof(RRQMConfigExtensions), "RRQMServer");

        /// <summary>
        /// 多线程数量，默认为10。
        /// <para>TCP模式中，该值等效于<see cref="ThreadPool.SetMinThreads(int, int)"/></para>
        /// <para>UDP模式中，该值为重叠IO并发数</para>
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty ThreadCountProperty =
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(RRQMConfigExtensions), 10);

        /// <summary>
        /// 服务名称，用于标识，无实际意义
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetServerName(this RRQMConfig config, string value)
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
        public static RRQMConfig SetThreadCount(this RRQMConfig config, int value)
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
            DependencyProperty.Register("BindIPHost", typeof(IPHost), typeof(RRQMConfigExtensions), null);

        /// <summary>
        /// 在Socket配置KeepAlive属性，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty KeepAliveValueProperty =
            DependencyProperty.Register("KeepAliveValue", typeof(KeepAliveValue), typeof(RRQMConfigExtensions), new KeepAliveValue());

        /// <summary>
        /// 数据包最大值。该值会在适当时间，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        public static readonly DependencyProperty MaxPackageSizeProperty =
           DependencyProperty.Register("MaxPackageSize", typeof(int), typeof(RRQMConfigExtensions), 1024 * 1024 * 10);

        /// <summary>
        /// 设置Socket不使用Delay算法，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty NoDelayProperty =
            DependencyProperty.Register("NoDelay", typeof(bool), typeof(RRQMConfigExtensions), false);

        /// <summary>
        /// 远程目标地址，所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty RemoteIPHostProperty =
            DependencyProperty.Register("RemoteIPHost", typeof(IPHost), typeof(RRQMConfigExtensions), null);

        /// <summary>
        /// 在异步发送时，使用独立线程发送，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty SeparateThreadSendProperty =
            DependencyProperty.Register("SeparateThreadSend", typeof(bool), typeof(RRQMConfigExtensions), false);

        /// <summary>
        /// Ssl配置，为Null时则不启用
        /// 所需类型<see cref="RRQMSocket.SslOption"/>
        /// </summary>
        public static readonly DependencyProperty SslOptionProperty =
            DependencyProperty.Register("SslOption", typeof(SslOption), typeof(RRQMConfigExtensions), null);

        /// <summary>
        /// 在Socket的KeepAlive属性。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetKeepAliveValue(this RRQMConfig config, KeepAliveValue value)
        {
            config.SetValue(KeepAliveValueProperty, value);
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
        public static RRQMConfig SetBindIPHost(this RRQMConfig config, IPHost value)
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
        public static RRQMConfig SetBindIPHost(this RRQMConfig config, int value)
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
        public static RRQMConfig SetBindIPHost(this RRQMConfig config, string value)
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
        public static RRQMConfig SetClientSslOption(this RRQMConfig config, ClientSslOption value)
        {
            config.SetValue(SslOptionProperty, value);
            return config;
        }

        /// <summary>
        /// 数据包最大值，默认1024*1024*10。该值会在适当时间，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetMaxPackageSize(this RRQMConfig config, int value)
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
        public static RRQMConfig SetRemoteIPHost(this RRQMConfig config, IPHost value)
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
        public static RRQMConfig SetRemoteIPHost(this RRQMConfig config, string value)
        {
            return SetRemoteIPHost(config, new IPHost(value));
        }


        /// <summary>
        /// 设置Socket的NoDelay属性，默认false。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static RRQMConfig UseNoDelay(this RRQMConfig config)
        {
            config.SetValue(NoDelayProperty, true);
            return config;
        }

        /// <summary>
        /// 在异步发送时，使用独立线程发送。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static RRQMConfig UseSeparateThreadSend(this RRQMConfig config)
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
            DependencyProperty.Register("Backlog", typeof(int), typeof(RRQMConfigExtensions), 100);

        /// <summary>
        /// 获取或设置清理无数据交互的SocketClient，默认60*1000 ms。如果不想清除，可使用-1。
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty ClearIntervalProperty =
            DependencyProperty.Register("ClearInterval", typeof(int), typeof(RRQMConfigExtensions), 60 * 1000);

        /// <summary>
        /// 清理统计类型。
        /// <para><see cref="ClearType.Receive"/>为在收到数据时，刷新统计，如果一直有数据接收，则不会被主动清理断开</para>
        /// <para><see cref="ClearType.Send"/>为在发送数据时，刷新统计，如果一直有数据发送，则不会被主动清理断开</para>
        /// <para>二者可叠加使用。</para>
        /// 所需类型<see cref="RRQMSocket.ClearType"/>
        /// </summary>
        public static readonly DependencyProperty ClearTypeProperty =
            DependencyProperty.Register("ClearType", typeof(ClearType), typeof(RRQMConfigExtensions), ClearType.Send | ClearType.Receive);

        /// <summary>
        /// 服务器负责监听的地址组。所需类型<see cref="IPHost"/>数组
        /// </summary>
        public static readonly DependencyProperty ListenIPHostsProperty =
            DependencyProperty.Register("ListenIPHosts", typeof(IPHost[]), typeof(RRQMConfigExtensions), null);

        /// <summary>
        /// 最大可连接数，默认为10000，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty MaxCountProperty =
            DependencyProperty.Register("MaxCount", typeof(int), typeof(RRQMConfigExtensions), 10000);

        /// <summary>
        /// 挂起连接队列的最大长度，默认100。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetBacklog(this RRQMConfig config, int value)
        {
            config.SetValue(BacklogProperty, value);
            return config;
        }

        /// <summary>
        /// 设置默认ID的获取方式，所需类型<see cref="Func{T, TResult}"/>
        /// </summary>
        public static readonly DependencyProperty GetDefaultNewIDProperty =
            DependencyProperty.Register("GetDefaultNewID", typeof(Func<string>), typeof(RRQMConfigExtensions), null);

        /// <summary>
        /// 设置默认ID的获取方式。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetGetDefaultNewID(this RRQMConfig config, Func<string> value)
        {
            config.SetValue(GetDefaultNewIDProperty, value);
            return config;
        }

        /// <summary>
        /// 设置清理无数据交互的SocketClient，默认60*1000 ms。如果不想清除，可使用-1
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetClearInterval(this RRQMConfig config, int value)
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
        public static RRQMConfig SetClearType(this RRQMConfig config, ClearType value)
        {
            config.SetValue(ClearTypeProperty, value);
            return config;
        }

        /// <summary>
        /// 服务器负责监听的地址组。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetListenIPHosts(this RRQMConfig config, IPHost[] value)
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
        public static RRQMConfig SetMaxCount(this RRQMConfig config, int value)
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
        public static RRQMConfig SetServiceSslOption(this RRQMConfig config, ServiceSslOption value)
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
            DependencyProperty.Register("EnableBroadcast", typeof(bool), typeof(RRQMConfigExtensions), false);

        /// <summary>
        /// 该值指定 System.Net.Sockets.Socket可以发送或接收广播数据包。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static RRQMConfig UseBroadcast(this RRQMConfig config)
        {
            config.SetValue(EnableBroadcastProperty, true);
            return config;
        }
        #endregion UDP

        #region 插件
        /// <summary>
        /// 添加插件。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="plugin"></param>
        /// <returns></returns>
        public static RRQMConfig AddPlugin(this RRQMConfig config, IPlugin plugin)
        {
            config.PluginsManager.Add(plugin);
            return config;
        }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static RRQMConfig AddPlugin<TPlugin>(this RRQMConfig config) where TPlugin : IPlugin
        {
            config.PluginsManager.Add(config.Container.Resolve<TPlugin>());
            return config;
        }
        #endregion

        #region 日志

        /// <summary>
        /// 设置日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static RRQMConfig SetLogger<TLogger>(this RRQMConfig config) where TLogger :class, ILog, new()
        {
            config.Container.RegisterTransient<ILog, TLogger>();
            return config;
        }

        /// <summary>
        /// 设置单例日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static RRQMConfig SetSingletonLogger<TLogger>(this RRQMConfig config) where TLogger : class, ILog, new()
        {
            config.Container.RegisterSingleton<ILog, TLogger>();
            return config;
        }

        /// <summary>
        /// 设置实例日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static RRQMConfig SetSingletonLogger<TLogger>(this RRQMConfig config, TLogger logger) where TLogger :class, ILog
        {
            config.Container.RegisterSingleton<ILog, TLogger>(logger);
            return config;
        }
        #endregion

        #region 创建
        /// <summary>
        /// 构建Tcp类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithTcpService<TService>(this RRQMConfig config) where TService : ITcpService
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
        public static TcpService BuildWithTcpService(this RRQMConfig config)
        {
            return BuildWithTcpService<TcpService>(config);
        }

        /// <summary>
        /// 构建Tcp类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithTcpClient<TClient>(this RRQMConfig config) where TClient : ITcpClient
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
        public static TcpClient BuildWithTcpClient(this RRQMConfig config)
        {
            return BuildWithTcpClient<TcpClient>(config);
        }

        /// <summary>
        /// 构建UDP类，并启动。
        /// </summary>
        /// <typeparam name="TSession"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TSession BuildWithUdpSession<TSession>(this RRQMConfig config) where TSession : IUdpSession
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
        public static UdpSession BuildWithUdpSession(this RRQMConfig config)
        {
            return BuildWithUdpSession<UdpSession>(config);
        }
        #endregion
    }
}