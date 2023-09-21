//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// SocketsServiceCollectionExtensions
    /// </summary>
    public static class SocketsServiceCollectionExtensions
    {
        #region TcpService

        /// <summary>
        /// 添加<see cref="ITcpService"/>服务。并且以TService作为注册接口。
        /// </summary>
        /// <typeparam name="TService">注册服务的接口</typeparam>
        /// <param name="service"></param>
        /// <param name="tcpService">实例化的服务器</param>
        /// <returns></returns>
        public static TService AddTcpService<TService>(this IServiceCollection service, ITcpService tcpService)
            where TService : class, ITcpService
        {
            service.AddSingleton((provider) => (TService)tcpService);
            return (TService)tcpService;
        }

        /// <summary>
        /// 添加<see cref="ITcpService"/>服务。
        /// </summary>
        /// <typeparam name="TService">注册服务的接口</typeparam>
        /// <param name="service"></param>
        /// <param name="configFunc">设置配置相关信息</param>
        /// <returns></returns>
        public static TService AddTcpService<TService>(this IServiceCollection service, Func<TouchSocketConfig> configFunc) where TService : class, ITcpService, new()
        {
            var tcpService = new TService();
            tcpService.Setup(configFunc.Invoke())
                .Start();

            return (TService)AddTcpService<ITcpService>(service, tcpService);
        }

        /// <summary>
        ///  以<see cref="ITcpService"/>作为注入接口，添加TcpService服务。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configFunc">设置配置相关信息</param>
        /// <returns></returns>
        public static ITcpService AddTcpService(this IServiceCollection service, Func<TouchSocketConfig> configFunc)
        {
            return AddTcpService<TcpService>(service, configFunc);
        }

        /// <summary>
        /// 以<see cref="ITcpService"/>作为注入接口，添加TcpService服务。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="port">监听端口</param>
        /// <returns></returns>
        public static ITcpService AddTcpService(this IServiceCollection service, int port)
        {
            return AddTcpService<TcpService>(service, () =>
            {
                return new TouchSocketConfig().SetListenIPHosts(new IPHost[] { new IPHost(port) });
            });
        }

        #endregion TcpService

        #region UdpSession

        /// <summary>
        /// 添加<see cref="UdpSession"/>服务。以TUdpSession泛型为注册接口。
        /// </summary>
        /// <typeparam name="TUdpSession">注册服务的接口</typeparam>
        /// <param name="service"></param>
        /// <param name="udpSession">实例化的UdpSession</param>
        /// <returns></returns>
        public static TUdpSession AddUdpSession<TUdpSession>(this IServiceCollection service, IUdpSession udpSession) where TUdpSession : class, IUdpSession
        {
            service.AddSingleton((p) => (TUdpSession)udpSession);
            return (TUdpSession)udpSession;
        }

        /// <summary>
        /// 添加<see cref="UdpSession"/>服务。以TUdpSession泛型为注册接口。
        /// </summary>
        /// <typeparam name="TUdpSession">注册服务的接口</typeparam>
        /// <param name="service"></param>
        /// <param name="configFunc"></param>
        /// <returns></returns>
        public static TUdpSession AddUdpSession<TUdpSession>(this IServiceCollection service, Func<TouchSocketConfig> configFunc) where TUdpSession : class, IUdpSession, new()
        {
            var udpSession = new TUdpSession();
            udpSession.Setup(configFunc.Invoke());
            udpSession.Start();
            return AddUdpSession<TUdpSession>(service, udpSession);
        }

        /// <summary>
        /// 添加<see cref="UdpSession"/>服务。以<see cref="IUdpSession"/>为注册接口。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configFunc"></param>
        /// <returns></returns>
        public static IUdpSession AddUdpSession(this IServiceCollection service, Func<TouchSocketConfig> configFunc)
        {
            var udpSession = new UdpSession();
            udpSession.Setup(configFunc.Invoke());
            udpSession.Start();
            return AddUdpSession<IUdpSession>(service, udpSession);
        }

        #endregion UdpSession
    }
}