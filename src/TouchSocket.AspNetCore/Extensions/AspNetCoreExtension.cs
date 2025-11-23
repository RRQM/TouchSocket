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

using System.Diagnostics.CodeAnalysis;
using TouchSocket.Dmtp;
using TouchSocket.Http;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// AspNetCoreContainerExtension
/// </summary>
public static class AspNetCoreExtension
{
    #region TcpDmtpService

    /// <summary>
    /// 添加TcpDmtpService服务。
    /// 此方法用于通过依赖注入将TcpDmtpService服务及其实现类注册到服务集合中。
    /// 它允许指定服务接口TService和该服务的具体实现类TImpService，
    /// 并通过提供的配置操作委托对相关配置进行定制。
    /// </summary>
    /// <typeparam name="TService">服务的接口类型。</typeparam>
    /// <typeparam name="TImpService">服务的具体实现类。</typeparam>
    /// <param name="services">服务集合，用于存储应用程序中所有注册的服务。</param>
    /// <param name="actionConfig">配置操作委托，用于定制TouchSocket的配置。</param>
    /// <returns>返回扩展后的服务集合，允许方法链式调用。</returns>
    public static IServiceCollection AddTcpDmtpService<TService, [DynamicallyAccessedMembers(AOT.Container)] TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TService : class, ITcpDmtpServiceBase
        where TImpService : class, TService
    {
        // 调用AddTcpService方法来注册TcpDmtpService服务，
        // 该方法是实际执行服务添加的地方。
        return services.AddTcpService<TService, TImpService>(actionConfig);
    }

    /// <summary>
    /// 添加TcpDmtpService服务。并使用<see cref="ITcpDmtpService"/>注册服务。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="actionConfig">配置操作委托</param>
    /// <returns>返回服务集合</returns>
    public static IServiceCollection AddTcpDmtpService(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        // 使用泛型方法AddTcpDmtpService，注册TcpDmtpService服务，实现ITcpDmtpService接口
        return services.AddTcpDmtpService<ITcpDmtpService, TcpDmtpService>(actionConfig);
    }

    #endregion TcpDmtpService

    #region TcpDmtpClient

    /// <summary>
    /// 添加Scoped TcpDmtpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddScopedTcpDmtpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, ITcpDmtpClient
        where TImpClient : class, TClient
    {
        return services.AddScopedSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加Scoped TcpDmtpClient服务。并使用<see cref="ITcpDmtpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddScopedTcpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddScopedTcpDmtpClient<ITcpDmtpClient, TcpDmtpClient>(actionConfig);
    }

    /// <summary>
    /// 添加单例TcpDmtpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddSingletonTcpDmtpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, ITcpDmtpClient
        where TImpClient : class, TClient
    {
        return services.AddSingletonSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加单例TcpDmtpClient服务。并使用<see cref="ITcpDmtpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddSingletonTcpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddSingletonTcpDmtpClient<ITcpDmtpClient, TcpDmtpClient>(actionConfig);
    }

    /// <summary>
    /// 添加瞬态TcpDmtpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddTransientTcpDmtpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, ITcpDmtpClient
        where TImpClient : class, TClient
    {
        return services.AddTransientSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加瞬态TcpDmtpClient服务。并使用<see cref="ITcpDmtpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddTransientTcpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddTransientTcpDmtpClient<ITcpDmtpClient, TcpDmtpClient>(actionConfig);
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
    public static IServiceCollection AddHttpService<TService, [DynamicallyAccessedMembers(AOT.Container)] TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TService : class, IHttpServiceBase
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
    /// 添加Scoped HttpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddScopedHttpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, IHttpClient
        where TImpClient : class, TClient
    {
        return services.AddScopedSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加Scoped HttpClient服务。并使用<see cref="IHttpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddScopedHttpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddScopedHttpClient<IHttpClient, HttpClient>(actionConfig);
    }

    /// <summary>
    /// 添加单例HttpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddSingletonHttpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, IHttpClient
        where TImpClient : class, TClient
    {
        return services.AddSingletonSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加单例HttpClient服务。并使用<see cref="IHttpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddSingletonHttpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddSingletonHttpClient<IHttpClient, HttpClient>(actionConfig);
    }

    /// <summary>
    /// 添加瞬态HttpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddTransientHttpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, IHttpClient
        where TImpClient : class, TClient
    {
        return services.AddTransientSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加瞬态HttpClient服务。并使用<see cref="IHttpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddTransientHttpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddTransientHttpClient<IHttpClient, HttpClient>(actionConfig);
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
    public static IServiceCollection AddHttpDmtpService<TService, [DynamicallyAccessedMembers(AOT.Container)] TImpService>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TService : class, IHttpDmtpServiceBase
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
    /// 添加Scoped HttpDmtpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddScopedHttpDmtpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, IHttpDmtpClient
        where TImpClient : class, TClient
    {
        return services.AddScopedSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加Scoped HttpDmtpClient服务。并使用<see cref="IHttpDmtpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddScopedHttpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddScopedHttpDmtpClient<IHttpDmtpClient, HttpDmtpClient>(actionConfig);
    }

    /// <summary>
    /// 添加单例HttpDmtpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddSingletonHttpDmtpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, IHttpDmtpClient
        where TImpClient : class, TClient
    {
        return services.AddSingletonSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加单例HttpDmtpClient服务。并使用<see cref="IHttpDmtpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddSingletonHttpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddSingletonHttpDmtpClient<IHttpDmtpClient, HttpDmtpClient>(actionConfig);
    }

    /// <summary>
    /// 添加瞬态HttpDmtpClient服务。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImpClient"></typeparam>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddTransientHttpDmtpClient<TClient, [DynamicallyAccessedMembers(AOT.Container)] TImpClient>(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
        where TClient : class, IHttpDmtpClient
        where TImpClient : class, TClient
    {
        return services.AddTransientSetupConfigObject<TClient, TImpClient>(actionConfig);
    }

    /// <summary>
    /// 添加瞬态HttpDmtpClient服务。并使用<see cref="IHttpDmtpClient"/>注册服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddTransientHttpDmtpClient(this IServiceCollection services, Action<TouchSocketConfig> actionConfig)
    {
        return services.AddTransientHttpDmtpClient<IHttpDmtpClient, HttpDmtpClient>(actionConfig);
    }

    #endregion HttpDmtpClient
}