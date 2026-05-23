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

using TouchSocket.Mcp;
using TouchSocket.Rpc;

namespace TouchSocket.Core;

/// <summary>
/// 提供 MCP HTTP 插件的 <see cref="IPluginManager"/> 扩展方法。
/// </summary>
public static class McpPluginsManagerExtension
{
    /// <summary>
    /// 添加基于 HTTP 的 MCP 服务端插件。
    /// </summary>
    /// <param name="pluginManager"><see cref="IPluginManager"/> 实例。</param>
    /// <param name="options">MCP HTTP 插件选项。</param>
    /// <returns>创建的 <see cref="McpHttpPlugin"/> 实例。</returns>
    public static McpHttpPlugin UseMcpHttpPlugin(this IPluginManager pluginManager, Action<McpHttpPluginOptions> options)
    {
        var option = new McpHttpPluginOptions();
        options.Invoke(option);

        var plugin = new McpHttpPlugin(pluginManager.Resolver.Resolve<IRpcServerProvider>(), option);
        pluginManager.Add(plugin);
        return plugin;
    }
}
