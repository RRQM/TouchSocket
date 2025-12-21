// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Mqtt;

/// <summary>
/// PluginManagerExtension
/// </summary>
public static class PluginManagerExtension
{
    /// <summary>
    /// 使用Mqtt WebSocket插件
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    /// <param name="configure">配置选项委托</param>
    public static void UseMqttWebSocket(this IPluginManager pluginManager, Action<MqttWebSocketFeatureOption> configure)
    {
        var options = new MqttWebSocketFeatureOption();
        configure?.Invoke(options);
        var service = pluginManager.Resolver.Resolve<IMqttWebSocketService>();
        if (service is null)
        {
            ThrowHelper.ThrowException($"未能从容器中解析{nameof(IMqttWebSocketService)}服务，请确保已注册该服务。");
        }
        var feature = new MqttWebSocketFeature(service, options);
        pluginManager.Add(feature);
    }

    /// <summary>
    /// 使用Mqtt WebSocket插件，指定Mqtt WebSocket的Url
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    /// <param name="url">Mqtt WebSocket的Url，默认为"/mqtt"</param>
    public static void UseMqttWebSocket(this IPluginManager pluginManager, string url = "/mqtt")
    {
        pluginManager.UseMqttWebSocket(options =>
        {
            options.SetUrl(url);
        });
    }
}
