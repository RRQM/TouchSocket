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

using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace TouchSocket.WebApi.Swagger;

/// <summary>
/// Swagger 插件管理器扩展
/// </summary>
public static class SwaggerPluginManagerExtension
{
    /// <summary>
    /// 使用 <see cref="SwaggerPlugin"/> 插件，同时自动添加 <see cref="OpenApiPlugin"/>。
    /// <see cref="OpenApiPlugin"/> 提供 openapi.json 端点，<see cref="SwaggerPlugin"/> 提供 Swagger UI 页面。
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    /// <param name="options">Swagger 配置选项</param>
    public static void UseSwagger(this IPluginManager pluginManager, Action<SwaggerOption> options)
    {
        var option = new SwaggerOption();
        options.Invoke(option);

        // 默认使用 SwaggerDescriptionAttribute 提供标签
        option.GetTags ??= GetTagsFromSwaggerDescription;

        var logger = pluginManager.Resolver.Resolve<ILog>();
        pluginManager.Add(new OpenApiPlugin(logger, option));
        pluginManager.Add(new SwaggerPlugin(logger, option));
    }

    /// <summary>
    /// 使用 <see cref="SwaggerPlugin"/> 插件，同时自动添加 <see cref="OpenApiPlugin"/>。
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    public static void UseSwagger(this IPluginManager pluginManager)
    {
        pluginManager.UseSwagger(options => { });
    }

    private static IEnumerable<string> GetTagsFromSwaggerDescription(RpcMethod rpcMethod)
    {
        var tags = new List<string>();

        foreach (var item in rpcMethod.ServerFromType.GetCustomAttributes(false))
        {
            if (item is SwaggerDescriptionAttribute attr && attr.Groups != null)
            {
                tags.AddRange(attr.Groups);
            }
        }

        foreach (var item in rpcMethod.Info.GetCustomAttributes(false))
        {
            if (item is SwaggerDescriptionAttribute attr && attr.Groups != null)
            {
                tags.AddRange(attr.Groups);
            }
        }

        if (tags.Count == 0)
        {
            tags.Add(rpcMethod.ServerFromType.Name);
        }

        return tags;
    }
}
