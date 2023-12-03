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
using TouchSocket.Dmtp;
using TouchSocket.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// AspNetCoreContainerExtension
    /// </summary>
    public static class AspNetCoreExtension
    {
        #region TcpDmtpService

        /// <summary>
        /// 添加TcpDmtpService服务。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpService"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpDmtpService<TService, TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TService : class, ITcpDmtpService
            where TImpService : class, TService
        {
            return services.AddTcpService<TService, TImpService>(actionConfig);
        }

        /// <summary>
        /// 添加TcpDmtpService服务。并使用<see cref="ITcpDmtpService"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpDmtpService(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddTcpDmtpService<ITcpDmtpService, TcpDmtpService>(actionConfig);
        }

        #endregion TcpDmtpService

        #region TcpDmtpClient

        /// <summary>
        /// 添加TcpDmtpClient服务。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TImpClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpDmtpClient<TClient, TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TClient : class, ITcpDmtpClient
            where TImpClient : class, TClient
        {
            return services.AddTcpClient<TClient, TImpClient>(actionConfig);
        }

        /// <summary>
        /// 添加TcpDmtpClient服务。并使用<see cref="ITcpDmtpClient"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddTcpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddTcpDmtpClient<ITcpDmtpClient, TcpDmtpClient>(actionConfig);
        }

        #endregion TcpDmtpClient

        #region HttpService

        /// <summary>
        /// 添加HttpService服务。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpService"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpService<TService, TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TService : class, IHttpService
            where TImpService : class, TService
        {
            return services.AddTcpService<TService, TImpService>(actionConfig);
        }

        /// <summary>
        /// 添加HttpService服务。并使用<see cref="IHttpService"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpService(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddHttpService<IHttpService, HttpService>(actionConfig);
        }

        #endregion HttpService

        #region HttpClient

        /// <summary>
        /// 添加HttpClient服务。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TImpClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpClient<TClient, TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TClient : class, IHttpClient
            where TImpClient : class, TClient
        {
            return services.AddTcpClient<TClient, TImpClient>(actionConfig);
        }

        /// <summary>
        /// 添加HttpClient服务。并使用<see cref="IHttpClient"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddHttpClient<IHttpClient, HttpClient>(actionConfig);
        }

        #endregion HttpClient

        #region HttpDmtpService

        /// <summary>
        /// 添加HttpDmtpService服务。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpService"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpDmtpService<TService, TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TService : class, IHttpDmtpService
            where TImpService : class, TService
        {
            return services.AddTcpService<TService, TImpService>(actionConfig);
        }

        /// <summary>
        /// 添加HttpDmtpService服务。并使用<see cref="IHttpDmtpService"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpDmtpService(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddHttpDmtpService<IHttpDmtpService, HttpDmtpService>(actionConfig);
        }

        #endregion HttpDmtpService

        #region HttpDmtpClient

        /// <summary>
        /// 添加HttpDmtpClient服务。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TImpClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpDmtpClient<TClient, TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
            where TClient : class, IHttpDmtpClient
            where TImpClient : class, TClient
        {
            return services.AddTcpClient<TClient, TImpClient>(actionConfig);
        }

        /// <summary>
        /// 添加HttpDmtpClient服务。并使用<see cref="IHttpDmtpClient"/>注册服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="actionConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        {
            return services.AddHttpDmtpClient<IHttpDmtpClient, HttpDmtpClient>(actionConfig);
        }

        #endregion HttpDmtpClient
    }
}