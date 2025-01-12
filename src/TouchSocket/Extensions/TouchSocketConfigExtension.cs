//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 触摸套接字配置扩展类
    /// </summary>
    public static class TouchSocketConfigExtension
    {
        #region 数据

        /// <summary>
        /// 发送超时设定，默认为0。
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty<int> SendTimeoutProperty =
            new("SendTimeout", 0);

        /// <summary>
        /// 数据处理适配器
        /// 所需类型<see cref="Func{TResult}"/>
        /// </summary>
        public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> TcpDataHandlingAdapterProperty = new("TcpDataHandlingAdapter", null);

        /// <summary>
        /// 数据处理适配器
        /// 所需类型<see cref="Func{TResult}"/>
        /// </summary>
        public static readonly DependencyProperty<Func<UdpDataHandlingAdapter>> UdpDataHandlingAdapterProperty = new("UdpDataHandlingAdapter", null);

        /// <summary>
        /// 最小缓存池尺寸
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty<int?> MinBufferSizeProperty = new("MinBufferSize", default);

        /// <summary>
        /// 最大缓存池尺寸
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty<int?> MaxBufferSizeProperty = new("MaxBufferSize", default);

        /// <summary>
        /// 最小缓存容量，默认缺省。
        /// <list type="number">
        /// </list>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetMinBufferSize(this TouchSocketConfig config, int value)
        {
            if (value < 1024)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(value), value, 1024);
            }
            config.SetValue(MinBufferSizeProperty, value);
            return config;
        }

        /// <summary>
        /// 最大缓存容量，默认缺省。
        /// <list type="number">
        /// </list>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetMaxBufferSize(this TouchSocketConfig config, int value)
        {
            if (value < 1024)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(value), value, 1024);
            }

            config.SetValue(MaxBufferSizeProperty, value);
            return config;
        }

        /// <summary>
        /// 发送超时设定，单位毫秒，默认为0。意为禁用该配置。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSendTimeout(this TouchSocketConfig config, int value)
        {
            config.SetValue(SendTimeoutProperty, value);
            return config;
        }

        /// <summary>
        /// 设置(Tcp系)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetTcpDataHandlingAdapter(this TouchSocketConfig config, Func<SingleStreamDataHandlingAdapter> value)
        {
            config.SetValue(TcpDataHandlingAdapterProperty, value);
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
        public static readonly DependencyProperty<string> ServerNameProperty = new("ServerName", "TouchSocketServer");

        /// <summary>
        /// 多线程数量。默认-1缺省。
        /// <para>UDP模式中，该值为重叠IO并发数</para>
        /// 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty<int> ThreadCountProperty = new("ThreadCount", -1);

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
        /// 多线程数量，默认为-1缺省，实际上在udp中相当于1。
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
        /// Tcp固定端口绑定，
        /// 所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty<IPHost> BindIPHostProperty = new("BindIPHost", null);

        /// <summary>
        /// 在Socket配置KeepAlive属性，这个是操作tcp底层的，如果你对底层不了解，建议不要动。
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty<KeepAliveValue> KeepAliveValueProperty = new("KeepAliveValue", default);

        /// <summary>
        /// 设置Socket不使用Delay算法，
        /// 所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty<bool?> NoDelayProperty = new("NoDelay", null);

        /// <summary>
        /// 远程目标地址，所需类型<see cref="IPHost"/>
        /// </summary>
        public static readonly DependencyProperty<IPHost> RemoteIPHostProperty = new("RemoteIPHost", null);

        /// <summary>
        /// Ssl配置，为Null时则不启用
        /// 所需类型<see cref="TouchSocket.Sockets.SslOption"/>
        /// </summary>
        public static readonly DependencyProperty<SslOption> SslOptionProperty = new("SslOption", null);

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
        /// <para>注意：这个是操作tcp底层的，如果你对底层不了解，建议不要动。</para>
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
        /// 设置远程目标地址。在<see cref="UdpSessionBase"/>中，表示默认发送时的目标地址。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetRemoteIPHost(this TouchSocketConfig config, IPHost value)
        {
            config.SetValue(RemoteIPHostProperty, value);
            if (value.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase)
                     || value.Scheme.Equals("wss", StringComparison.CurrentCultureIgnoreCase)
                     || value.Scheme.Equals("ssl", StringComparison.CurrentCultureIgnoreCase)
                     || value.Scheme.Equals("tls", StringComparison.CurrentCultureIgnoreCase))
            {
                config.SetClientSslOption(new ClientSslOption()
                {
                    TargetHost = value.Authority,
#if NET45 || NETSTANDARD2_0
                    SslProtocols = SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
#elif NET481_OR_GREATER
                    SslProtocols = SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13
#elif NET6_0
                    SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13
#elif NET7_0_OR_GREATER
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
#endif
                });
            }
            return config;
        }

        /// <summary>
        /// 设置Socket的NoDelay属性，默认不做处理。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetNoDelay(this TouchSocketConfig config, bool value)
        {
            config.SetValue(NoDelayProperty, value);
            return config;
        }

        #endregion TcpClient

        #region TcpService

        /// <summary>
        /// 挂起连接队列的最大长度，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty<int?> BacklogProperty = new("Backlog", null);

        /// <summary>
        /// 设置默认Id的获取方式，所需类型<see cref="Func{T, TResult}"/>
        /// </summary>
        public static readonly DependencyProperty<Func<string>> GetDefaultNewIdProperty = new("GetDefaultNewId", null);

        /// <summary>
        /// 服务器负责监听的地址组。所需类型<see cref="IPHost"/>数组
        /// </summary>
        public static readonly DependencyProperty<IPHost[]> ListenIPHostsProperty = new("ListenIPHosts", null);

        /// <summary>
        /// 直接单个配置服务器监听的地址组。所需类型<see cref="Action"/>
        /// </summary>
        public static readonly DependencyProperty<Action<List<TcpListenOption>>> ListenOptionsProperty = new("ListenOptions", null);

        /// <summary>
        /// 最大可连接数，默认为10000，所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty<int> MaxCountProperty = new("MaxCount", 10000);

        /// <summary>
        /// 端口复用，默认为false，所需类型<see cref="bool"/>
        /// </summary>
        public static readonly DependencyProperty<bool> ReuseAddressProperty = new("ReuseAddress", false);

        /// <summary>
        /// 挂起连接队列的最大长度，默认不设置值。
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
        /// 设置Tcp服务器默认Id的获取方式。仅服务器生效。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetGetDefaultNewId(this TouchSocketConfig config, Func<string> value)
        {
            config.SetValue(GetDefaultNewIdProperty, value);
            return config;
        }

        /// <summary>
        /// 服务器负责监听的地址组。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetListenIPHosts(this TouchSocketConfig config, params IPHost[] values)
        {
            config.SetValue(ListenIPHostsProperty, values);
            return config;
        }

        /// <summary>
        /// 直接单个配置服务器监听的地址组。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetListenOptions(this TouchSocketConfig config, Action<List<TcpListenOption>> value)
        {
            config.SetValue(ListenOptionsProperty, value);
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

        #endregion TcpService

        #region UDP

        /// <summary>
        /// 该值指定 System.Net.Sockets.Socket可以发送或接收广播数据包。
        /// </summary>
        public static readonly DependencyProperty<bool> EnableBroadcastProperty = new("EnableBroadcast", false);

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

        /// <summary>
        /// 当udp作为客户端时，开始接收数据。起作用相当于<see cref="SetBindIPHost(TouchSocketConfig, IPHost)"/>随机端口。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TouchSocketConfig UseUdpReceive(this TouchSocketConfig config)
        {
            return SetBindIPHost(config, 0);
        }

        /// <summary>
        /// 解决Windows下UDP连接被重置错误10054。
        /// </summary>
#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
        public static readonly DependencyProperty<bool> UdpConnResetProperty = new("UdpConnReset", false);

        /// <summary>
        /// 解决Windows下UDP连接被重置错误10054。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

        public static TouchSocketConfig UseUdpConnReset(this TouchSocketConfig config)
        {
            config.SetValue(UdpConnResetProperty, true);
            return config;
        }

        #endregion UDP

        #region 创建

        /// <summary>
        /// 构建可配置，可连接类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static async Task<TClient> BuildClientAsync<TClient>(this TouchSocketConfig config) where TClient : ISetupConfigObject, IConnectableClient, new()
        {
            var client = new TClient();
            await client.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            await client.ConnectAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return client;
        }

        /// <summary>
        /// 构建Tcp类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static async Task<TService> BuildServiceAsync<TService>(this TouchSocketConfig config) where TService : IServiceBase, new()
        {
            var service = new TService();
            await service.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            await service.StartAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return service;
        }
        #endregion 创建
    }
}