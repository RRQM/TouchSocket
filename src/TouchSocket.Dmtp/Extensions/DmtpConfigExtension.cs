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

using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpConfigExtensions
    /// </summary>
    public static class DmtpConfigExtension
    {
        /// <summary>
        /// 设置Dmtp相关配置。
        /// </summary>
        public static readonly DependencyProperty<DmtpOption> DmtpOptionProperty =
            DependencyProperty<DmtpOption>.Register("DmtpOption", new DmtpOption());

        /// <summary>
        /// 设置Dmtp相关配置。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetDmtpOption(this TouchSocketConfig config, DmtpOption value)
        {
            config.SetValue(DmtpOptionProperty, value);
            return config;
        }

        //#region 创建TcpDmtp

        ///// <summary>
        ///// 构建<see cref="TcpDmtpClient"/>类客户端，并连接
        ///// </summary>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static TClient BuildWithTcpDmtpClient<TClient>(this TouchSocketConfig config) where TClient : ITcpDmtpClient, new()
        //{
        //    return config.BuildClientAsync<TClient>();
        //}

        ///// <summary>
        ///// 构建<see cref="TcpDmtpClient"/>类客户端，并连接
        ///// </summary>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static TcpDmtpClient BuildWithTcpDmtpClient(this TouchSocketConfig config)
        //{
        //    return BuildWithTcpDmtpClient<TcpDmtpClient>(config);
        //}

        ///// <summary>
        ///// 构建<see cref="ITcpDmtpServiceBase"/>类服务器，并启动。
        ///// </summary>
        ///// <typeparam name="TService"></typeparam>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static TService BuildWithTcpDmtpService<TService>(this TouchSocketConfig config) where TService : ITcpDmtpServiceBase, new()
        //{
        //    return config.BuildService<TService>();
        //}

        ///// <summary>
        ///// 构建<see cref="ITcpDmtpServiceBase"/>类服务器，并启动。
        ///// </summary>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static TcpDmtpService BuildWithTcpDmtpService(this TouchSocketConfig config)
        //{
        //    return config.BuildService<TcpDmtpService>();
        //}

        //#endregion 创建TcpDmtp

        //#region 创建HttpDmtp

        ///// <summary>
        ///// 构建<see cref="IHttpDmtpClient"/>类客户端，并连接
        ///// </summary>
        ///// <typeparam name="TClient"></typeparam>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static TClient BuildWithHttpDmtpClient<TClient>(this TouchSocketConfig config) where TClient : IHttpDmtpClient, new()
        //{
        //    return config.BuildClientAsync<TClient>();
        //}

        ///// <summary>
        ///// 构建<see cref="IHttpDmtpClient"/>类客户端，并连接
        ///// </summary>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static HttpDmtpClient BuildWithHttpDmtpClient(this TouchSocketConfig config)
        //{
        //    return config.BuildClientAsync<HttpDmtpClient>();
        //}

        ///// <summary>
        ///// 构建<see cref="IHttpDmtpServiceBase"/>类服务器，并启动。
        ///// </summary>
        ///// <typeparam name="TService"></typeparam>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static TService BuildWithHttpDmtpService<TService>(this TouchSocketConfig config) where TService : IHttpDmtpServiceBase, new()
        //{
        //    return config.BuildService<TService>();
        //}

        ///// <summary>
        ///// 构建<see cref="IHttpDmtpServiceBase"/>类服务器，并启动。
        ///// </summary>
        ///// <param name="config"></param>
        ///// <returns></returns>
        //public static HttpDmtpService BuildWithHttpDmtpService(this TouchSocketConfig config)
        //{
        //    return config.BuildService<HttpDmtpService>();
        //}

        //#endregion 创建HttpDmtp

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