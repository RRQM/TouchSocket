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

namespace TouchSocket.Dmtp;

/// <summary>
/// DmtpRouteServiceExtension
/// </summary>
public static class DmtpRouteServiceExtension
{
    /// <summary>
    /// 添加Dmtp路由服务。
    /// </summary>
    /// <param name="registrator"></param>
    public static void AddDmtpRouteService(this IRegistrator registrator)
    {
        registrator.RegisterSingleton<IDmtpRouteService, DmtpRouteService>();
    }

    /// <summary>
    /// 扩展方法用于在服务容器中注册DMTP路由服务的单例实例。
    /// </summary>
    /// <typeparam name="TDmtpRouteService">DMTP路由服务的具体类型。</typeparam>
    /// <param name="registrator">服务注册器接口，用于在服务容器中注册服务。</param>
    public static void AddDmtpRouteService<TDmtpRouteService>(this IRegistrator registrator)
        where TDmtpRouteService : class, IDmtpRouteService
    {
        // 使用单例模式注册DMTP路由服务，确保在整个应用生命周期中只创建一个实例。
        registrator.RegisterSingleton<IDmtpRouteService, TDmtpRouteService>();
    }

    /// <summary>
    /// 添加基于设定委托的Dmtp路由服务。
    /// </summary>
    /// <param name="registrator"></param>
    /// <param name="func"></param>
    public static void AddDmtpRouteService(this IRegistrator registrator, Func<string, Task<IDmtpActor>> func)
    {
        registrator.RegisterSingleton<IDmtpRouteService>(new DmtpRouteService()
        {
            FindDmtpActor = func
        });
    }

    /// <summary>
    /// 添加基于设定委托的Dmtp路由服务。
    /// </summary>
    /// <param name="registrator">服务注册器接口，用于注册服务。</param>
    /// <param name="action">一个函数委托，根据ID返回一个IDmtpActor实例。</param>
    public static void AddDmtpRouteService(this IRegistrator registrator, Func<string, IDmtpActor> action)
    {
        // 调用重载版本的AddDmtpRouteService方法，处理异步操作
        AddDmtpRouteService(registrator, async (id) =>
        {
            // 完成一个已经完成的任务，用于简化异步操作
            await EasyTask.CompletedTask;
            // 调用传入的委托，并返回结果
            return action.Invoke(id);
        });
    }
}