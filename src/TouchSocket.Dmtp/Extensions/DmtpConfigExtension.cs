using System;
using System.Net.Sockets;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpConfigExtensions
    /// </summary>
    public static class DmtpConfigExtension
    {
        /// <summary>
        /// 默认使用Id。
        /// </summary>
        public static readonly DependencyProperty<string> DefaultIdProperty =
            DependencyProperty<string>.Register("DefaultId", null);

        /// <summary>
        /// DmtpClient连接时的元数据, 所需类型<see cref="Metadata"/>
        /// </summary>
        public static readonly DependencyProperty<Metadata> MetadataProperty = DependencyProperty<Metadata>.Register("Metadata", null);

        /// <summary>
        /// 验证超时时间,默认为3000ms, 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty<int> VerifyTimeoutProperty =
            DependencyProperty<int>.Register("VerifyTimeout", 3000);

        /// <summary>
        /// 连接令箭,当为null或空时，重置为默认值“rrqm”, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty<string> VerifyTokenProperty =
            DependencyProperty<string>.Register("VerifyToken", "rrqm");

        /// <summary>
        /// 设置默认的使用Id。仅在DmtpRpc组件适用。
        /// <para>
        /// 使用该功能时，仅在服务器的Handshaking之后生效。且如果id重复，则会连接失败。
        /// </para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetDefaultId(this TouchSocketConfig config, string value)
        {
            config.SetValue(DefaultIdProperty, value);
            return config;
        }

        /// <summary>
        /// 设置DmtpClient连接时的元数据
        /// <para>仅适用于DmtpClient系类</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetMetadata(this TouchSocketConfig config, Metadata value)
        {
            config.SetValue(MetadataProperty, value);
            return config;
        }

        /// <summary>
        /// 验证超时时间,默认为3000ms.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetVerifyTimeout(this TouchSocketConfig config, int value)
        {
            config.SetValue(VerifyTimeoutProperty, value);
            return config;
        }

        /// <summary>
        /// 连接令箭，当为null或空时，重置为默认值“rrqm”
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetVerifyToken(this TouchSocketConfig config, string value)
        {
            config.SetValue(VerifyTokenProperty, value);
            return config;
        }

        #region 创建TcpDmtp

        /// <summary>
        /// 构建<see cref="TcpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithTcpDmtpClient<TClient>(this TouchSocketConfig config)where TClient : ITcpDmtpClient,new ()
        {
            return config.BuildClient<TClient>();
        }

        /// <summary>
        /// 构建<see cref="TcpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpDmtpClient BuildWithTcpDmtpClient(this TouchSocketConfig config)
        {
            return BuildWithTcpDmtpClient<TcpDmtpClient>(config);
        }

        /// <summary>
        /// 构建<see cref="ITcpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithTcpDmtpService<TService>(this TouchSocketConfig config) where TService : ITcpDmtpService,new ()
        {
            return config.BuildService<TService>();
        }

        /// <summary>
        /// 构建<see cref="ITcpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpDmtpService BuildWithTcpDmtpService(this TouchSocketConfig config)
        {
            return config.BuildService<TcpDmtpService>();
        }

        #endregion 创建TcpDmtp

        #region 创建HttpDmtp

        /// <summary>
        /// 构建<see cref="IHttpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithHttpDmtpClient<TClient>(this TouchSocketConfig config) where TClient : IHttpDmtpClient,new ()
        {
            return config.BuildClient<TClient>();
        }

        /// <summary>
        /// 构建<see cref="IHttpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpDmtpClient BuildWithHttpDmtpClient(this TouchSocketConfig config)
        {
            return config.BuildClient<HttpDmtpClient>();
        }

        /// <summary>
        /// 构建<see cref="IHttpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithHttpDmtpService<TService>(this TouchSocketConfig config) where TService : IHttpDmtpService,new ()
        {
            return config.BuildService<TService>();
        }

        /// <summary>
        /// 构建<see cref="IHttpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpDmtpService BuildWithHttpDmtpService(this TouchSocketConfig config)
        {
            return config.BuildService<HttpDmtpService>();
        }

        #endregion 创建HttpDmtp

        //#region 创建UdpTouchRpc

        ///// <summary>
        ///// 构建UdpTouchRpc类
        ///// </summary>
        ///// <typeparam name="TClient"></typeparam>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static TClient BuildWithUdpTouchRpc<TClient>(this TouchSocketConfig config) where TClient : IUdpTouchRpc
        //{
        //    TClient client = Activator.CreateInstance<TClient>();
        //    client.Setup(config);
        //    client.Start();
        //    return client;
        //}

        ///// <summary>
        ///// 构建UdpTouchRpc类客户端
        ///// </summary>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static UdpTouchRpc BuildWithUdpTouchRpc(this TouchSocketConfig config)
        //{
        //    return BuildWithUdpTouchRpc<UdpTouchRpc>(config);
        //}

        //#endregion 创建UdpTouchRpc
    }
}