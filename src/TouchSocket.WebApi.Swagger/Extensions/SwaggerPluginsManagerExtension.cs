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

namespace TouchSocket.WebApi.Swagger;

/// <summary>
/// <inheritdoc/>
/// </summary>
public static class SwaggerPluginManagerExtension
{
    /// <summary>
    /// 使用<see cref="SwaggerPlugin"/>插件。
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    /// <param name="options">Swagger配置选项</param>
    /// <returns>Swagger插件实例</returns>
    public static void UseSwagger(this IPluginManager pluginManager, Action<SwaggerOption> options)
    {
        SwaggerOption option = new();
        options.Invoke(option);
        SwaggerPlugin swaggerPlugin = new(pluginManager.Resolver.Resolve<ILog>(), option);
        pluginManager.Add(swaggerPlugin);
    }

    /// <summary>
    /// 使用<see cref="SwaggerPlugin"/>插件。
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    /// <returns>Swagger插件实例</returns>
    public static void UseSwagger(this IPluginManager pluginManager)
    {
        pluginManager.UseSwagger(options => { });
    }
}