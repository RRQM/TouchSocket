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

using System;
using TouchSocket.Core;
using TouchSocket.Core.AspNetCore;
using TouchSocket.Hosting;
using TouchSocket.Hosting.Sockets.HostService;
using TouchSocket.Sockets;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ServiceCollectionExtensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        #region HostedService

        /// <summary>
        /// 添加Client类型的HostedService服务。
        /// <para>
        /// 这类服务必须实现<see cref="ISetupConfigObject"/>与<see cref="ICloseObject"/>。同时，服务不做其他处理。例如连接，需要自己调用。或者做无人值守重连。
        /// </para>
        /// </summary>
        /// <typeparam name="TObjectClient"></typeparam>
        /// <typeparam name="TObjectImpClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientHostedService<TObjectClient, TObjectImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TObjectClient : class, ISetupConfigObject, ICloseObject
            where TObjectImpClient : class, TObjectClient
        {
            return AddSetupConfigObjectHostedService<ClientHost<TObjectClient>, TObjectClient, TObjectImpClient>(services, actionConfig);
        }

        /// <summary>
        /// 添加Service类型的HostedService服务。这类服务必须实现<see cref="ISetupConfigObject"/>与<see cref="IService"/>
        /// </summary>
        /// <typeparam name="TObjectService"></typeparam>
        /// <typeparam name="TObjectImpService"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddServiceHostedService<TObjectService, TObjectImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TObjectService : class, ISetupConfigObject, IService
            where TObjectImpService : class, TObjectService
        {
            return AddSetupConfigObjectHostedService<ServiceHost<TObjectService>, TObjectService, TObjectImpService>(services, actionConfig);
        }

        /// <summary>
        /// 添加SetupConfigObjectHostedService服务。
        /// </summary>
        /// <typeparam name="THostService"></typeparam>
        /// <typeparam name="TObjectService"></typeparam>
        /// <typeparam name="TObjectImpService"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddSetupConfigObjectHostedService<THostService, TObjectService, TObjectImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where THostService : SetupConfigObjectHostedService<TObjectService>, new()
            where TObjectService : class, ISetupConfigObject
            where TObjectImpService : class, TObjectService
        {
            var aspNetCoreContainer = new AspNetCoreContainer(services);
            var config = new TouchSocketConfig();
            config.SetRegistrator(aspNetCoreContainer);
            actionConfig.Invoke(config);
            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IRegistrator> actionContainer)
            {
                actionContainer.Invoke(aspNetCoreContainer);
            }

            services.AddSingleton<TObjectService, TObjectImpService>();
            services.AddHostedService(privoder =>
            {
                var serviceHost = new THostService();
                serviceHost.SetObject(privoder.GetService<TObjectService>());
                serviceHost.SetConfig(config);
                serviceHost.SetProvider(privoder, aspNetCoreContainer);
                return serviceHost;
            });
            return services;
        }

        #endregion HostedService

        #region TcpService

        /// <summary>
        /// 添加TcpService服务。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpService"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpService<TService, TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TService : class, ITcpServiceBase
            where TImpService : class, TService
        {
            return AddServiceHostedService<TService, TImpService>(services, actionConfig);
        }

        /// <summary>
        /// 添加TcpService服务。并使用<see cref="ITcpService"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpService(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddTcpService<ITcpService, TcpService>(actionConfig);
        }

        #endregion TcpService

        #region UdpSession

        /// <summary>
        /// 添加UdpSession服务。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpService"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddUdpSession<TService, TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
           where TService : class, IUdpSession
           where TImpService : class, TService
        {
            return AddServiceHostedService<TService, TImpService>(services, actionConfig);
        }

        /// <summary>
        /// 添加Udp服务。并使用<see cref="IUdpSession"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddUdpSession(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddUdpSession<IUdpSession, UdpSession>(actionConfig);
        }

        #endregion UdpSession

        #region TcpClient

        /// <summary>
        /// 添加TcpClient服务。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TImpClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpClient<TClient, TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TClient : class, ITcpClient
            where TImpClient : class, TClient
        {
            return AddClientHostedService<TClient, TImpClient>(services, actionConfig);
        }

        /// <summary>
        /// 添加TcpClient服务。并使用<see cref="ITcpClient"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddTcpClient<ITcpClient, TcpClient>(actionConfig);
        }

        #endregion TcpClient
    }
}