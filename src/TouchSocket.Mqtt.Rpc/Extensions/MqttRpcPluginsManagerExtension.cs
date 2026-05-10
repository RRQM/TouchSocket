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

using TouchSocket.Mqtt.Rpc;
using TouchSocket.Rpc;

namespace TouchSocket.Core;

/// <summary>
/// MqttRpc 插件管理器扩展。
/// </summary>
public static class MqttRpcPluginsManagerExtension
{
    /// <summary>
    /// 使用 MqttRpc 插件（服务端+客户端模式）。
    /// </summary>
    /// <param name="pluginManager">插件管理器。</param>
    /// <param name="options">配置选项委托。</param>
    /// <returns>返回 <see cref="MqttRpcParserPlugin"/> 实例。</returns>
    public static MqttRpcParserPlugin UseMqttRpc(this IPluginManager pluginManager, Action<MqttRpcOption> options)
    {
        var option = new MqttRpcOption();
        options?.Invoke(option);

        var plugin = new MqttRpcParserPlugin(pluginManager.Resolver.Resolve<IRpcServerProvider>(), option);
        pluginManager.Add(plugin);
        return plugin;
    }

    /// <summary>
    /// 使用 MqttRpc 插件（服务端+客户端模式），使用默认配置。
    /// </summary>
    /// <param name="pluginManager">插件管理器。</param>
    /// <returns>返回 <see cref="MqttRpcParserPlugin"/> 实例。</returns>
    public static MqttRpcParserPlugin UseMqttRpc(this IPluginManager pluginManager)
    {
        return UseMqttRpc(pluginManager, null);
    }

    /// <summary>
    /// 使用 MqttRpc 插件（纯客户端模式，不注册 RPC 服务）。
    /// </summary>
    /// <param name="pluginManager">插件管理器。</param>
    /// <param name="options">配置选项委托。</param>
    /// <returns>返回 <see cref="MqttRpcParserPlugin"/> 实例。</returns>
    public static MqttRpcParserPlugin UseMqttRpcClient(this IPluginManager pluginManager, Action<MqttRpcOption> options)
    {
        var option = new MqttRpcOption();
        options?.Invoke(option);

        var plugin = new MqttRpcParserPlugin(null, option);
        pluginManager.Add(plugin);
        return plugin;
    }

    /// <summary>
    /// 使用 MqttRpc 插件（纯客户端模式，不注册 RPC 服务），使用默认配置。
    /// </summary>
    /// <param name="pluginManager">插件管理器。</param>
    /// <returns>返回 <see cref="MqttRpcParserPlugin"/> 实例。</returns>
    public static MqttRpcParserPlugin UseMqttRpcClient(this IPluginManager pluginManager)
    {
        return UseMqttRpcClient(pluginManager, null);
    }
}
