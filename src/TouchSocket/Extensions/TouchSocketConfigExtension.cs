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

namespace TouchSocket.Sockets;



/// <summary>
/// 触摸套接字配置扩展类
/// </summary>
public static class TouchSocketConfigExtension
{
    #region 数据

    /// <summary>
    /// 传输选项配置属性，类型为<see cref="TransportOption"/>。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<TransportOption> TransportOptionProperty = new("TransportOption", new TransportOption());


    /// <summary>
    /// 数据处理适配器
    /// 所需类型<see cref="Func{TResult}"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> TcpDataHandlingAdapterProperty = new("TcpDataHandlingAdapter", null);

    /// <summary>
    /// 数据处理适配器
    /// 所需类型<see cref="Func{TResult}"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<Func<UdpDataHandlingAdapter>> UdpDataHandlingAdapterProperty = new("UdpDataHandlingAdapter", null);

    #endregion 数据

    #region ServiceBase

    /// <summary>
    /// 服务名称，用于标识，无实际意义，所需类型<see cref="string"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<string> ServerNameProperty = new("ServerName", "TouchSocketServer");

    /// <summary>
    /// 重叠IO并发数。默认1。
    /// <para>UDP模式中，该值为重叠IO并发数</para>
    /// 所需类型<see cref="int"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<int> UdpOverlappedCountProperty = new("UdpOverlappedCount", 1);

    #endregion ServiceBase

    #region TcpClient

    /// <summary>
    /// Tcp固定端口绑定，
    /// 所需类型<see cref="IPHost"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<IPHost> BindIPHostProperty = new("BindIPHost", null);

    /// <summary>
    /// 在Socket配置KeepAlive属性，这个是操作tcp底层的，如果你对底层不了解，建议不要动。
    /// 所需类型<see cref="bool"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<KeepAliveValue> KeepAliveValueProperty = new("KeepAliveValue", default);

    /// <summary>
    /// 设置Socket不使用Delay算法，
    /// 所需类型<see cref="bool"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<bool> NoDelayProperty = new("NoDelay", true);

    /// <summary>
    /// 远程目标地址，所需类型<see cref="IPHost"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<IPHost> RemoteIPHostProperty = new("RemoteIPHost", null);

    /// <summary>
    /// ClientSslOption配置，为Null时则不启用
    /// 所需类型<see cref="TouchSocket.Sockets.SslOption"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<ClientSslOption> ClientSslOptionProperty = new("ClientSslOption", null);

    /// <summary>
    /// ServiceSslOption配置，为Null时则不启用
    /// 所需类型<see cref="TouchSocket.Sockets.SslOption"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<ServiceSslOption> ServiceSslOptionProperty = new("ServiceSslOption", null);

    #endregion TcpClient

    #region TcpService

    /// <summary>
    /// 挂起连接队列的最大长度，所需类型<see cref="int"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<int> BacklogProperty = new("Backlog", 100);

    /// <summary>
    /// 设置默认Id的获取方式，所需类型<see cref="Func{T, TResult}"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<Func<IClient, string>> GetDefaultNewIdProperty = new("GetDefaultNewId", null);

    /// <summary>
    /// 服务器负责监听的地址组。所需类型<see cref="IPHost"/>数组
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<IPHost[]> ListenIPHostsProperty = new("ListenIPHosts", null);

    /// <summary>
    /// 直接单个配置服务器监听的地址组。所需类型<see cref="Action"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<List<TcpListenOption>> ListenOptionsProperty = new("ListenOptions", null);

    /// <summary>
    /// 最大可连接数，默认为10000，所需类型<see cref="int"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<int> MaxCountProperty = new("MaxCount", 10000);

    /// <summary>
    /// 端口复用，默认为<see langword="false"/>，所需类型<see cref="bool"/>
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<bool> ReuseAddressProperty = new("ReuseAddress", false);

    #endregion TcpService

    #region UDP

    /// <summary>
    /// 该值指定 System.Net.Sockets.Socket可以发送或接收广播数据包。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<bool> EnableBroadcastProperty = new("EnableBroadcast", false);

    /// <summary>
    /// 当udp作为客户端时，开始接收数据。起作用相当于<see cref="BindIPHostProperty"/>0端口。
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static TouchSocketConfig UseUdpReceive(this TouchSocketConfig config)
    {
        return config.SetBindIPHost(0);
    }

    /// <summary>
    /// 解决Windows下UDP连接被重置错误10054。
    /// </summary>
#if NET6_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig))]
    public static readonly DependencyProperty<bool> UdpConnResetProperty = new("UdpConnReset", false);

    #endregion UDP

    #region 创建

    /// <summary>
    /// 构建可配置，可连接类客户端，并连接
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="config"></param>
    /// <returns></returns>
    public static async Task<TClient> BuildClientAsync<TClient>(this TouchSocketConfig config, CancellationToken cancellationToken = default) where TClient : ISetupConfigObject, IConnectableClient, new()
    {
        var client = new TClient();
        await client.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await client.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        return client;
    }

    /// <summary>
    /// 构建Tcp类服务器，并启动。
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="config"></param>
    /// <returns></returns>
    public static async Task<TService> BuildServiceAsync<TService>(this TouchSocketConfig config, CancellationToken cancellationToken = default) where TService : IServiceBase, new()
    {
        var service = new TService();
        await service.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await service.StartAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        return service;
    }
    #endregion 创建
}