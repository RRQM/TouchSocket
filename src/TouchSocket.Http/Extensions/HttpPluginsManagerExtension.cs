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

using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Core;

/// <summary>
/// HttpPluginManagerExtension
/// </summary>
public static class HttpPluginManagerExtension
{
    /// <summary>
    /// 使用默认的Http服务插件。该插件作为Http请求的默认处理者，用于拦截未被其他插件处理的Http请求。
    /// 该方法通过扩展方法的方式，允许插件管理器动态添加此默认Http服务插件。
    /// </summary>
    /// <param name="pluginManager">插件管理器，负责管理和添加插件。</param>
    /// <returns>返回创建并添加到插件管理器的默认Http服务插件实例。</returns>
    public static DefaultHttpServicePlugin UseDefaultHttpServicePlugin(this IPluginManager pluginManager)
    {
        var plugin = new DefaultHttpServicePlugin();
        pluginManager.Add(plugin);
        return plugin;
    }

    /// <summary>
    /// 静态页面插件扩展方法
    /// </summary>
    /// <param name="pluginManager">插件管理器实例</param>
    /// <param name="optionsAction">配置操作委托，用于配置静态页面选项</param>
    /// <returns>配置后的静态页面插件实例</returns>
    public static HttpStaticPagePlugin UseHttpStaticPage(this IPluginManager pluginManager, Action<StaticPageOptions> optionsAction)
    {
        // 创建静态页面选项实例
        var options = new StaticPageOptions();
        // 调用传入的委托以配置静态页面选项
        optionsAction(options);
        // 使用配置后的选项创建静态页面插件实例
        var plugin = new HttpStaticPagePlugin(options);
        // 将创建的插件添加到插件管理器中
        pluginManager.Add(plugin);
        // 返回配置后的插件实例
        return plugin;
    }

    /// <summary>
    /// 静态方法，用于通过插件管理器启用HTTP静态页面服务插件
    /// </summary>
    /// <param name="pluginManager">插件管理器对象，用于添加和管理插件</param>
    /// <returns>返回创建的HTTP静态页面服务插件实例</returns>
    public static HttpStaticPagePlugin UseHttpStaticPage(this IPluginManager pluginManager)
    {
        // 创建一个新的HTTP静态页面服务插件实例，使用默认的静态页面选项
        var plugin = new HttpStaticPagePlugin(new StaticPageOptions());
        // 将创建的插件实例添加到插件管理器中进行管理
        pluginManager.Add(plugin);
        // 返回插件实例
        return plugin;
    }

    /// <summary>
    /// 启用跨域功能
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    /// <param name="policyName">跨域策略名称</param>
    /// <returns>返回一个新的CorsPlugin实例</returns>
    public static CorsPlugin UseCors(this IPluginManager pluginManager, string policyName)
    {
        // 为插件管理器添加一个CorsPlugin，使用给定的跨域策略名称
        return pluginManager.Add(resolver => new CorsPlugin(resolver.Resolve<ICorsService>(), policyName));
    }


    /// <summary>
    /// 启用HttpSession的定期检查与清理插件。
    /// 该插件用于定期检查<see cref="IHttpSession"/>会话并进行清理操作，防止资源泄漏。
    /// </summary>
    /// <param name="pluginManager">插件管理器实例，用于添加和管理插件。</param>
    /// <returns>返回创建并添加到插件管理器的实例。</returns>
    public static CheckClearPlugin<IHttpSession> UseHttpSessionCheckClear(this IPluginManager pluginManager, Action<CheckClearOption<IHttpSession>> options = null)
    {
        return pluginManager.UseCheckClear<IHttpSession>(options);
    }
}