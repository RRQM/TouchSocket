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

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// <see cref="IPluginManager"/>的扩展方法，用于使用DmtpRpc插件。
/// </summary>
public static class DmtpRpcPluginManagerExtension
{
    /// <summary>
    /// 使用DmtpRpc插件。
    /// </summary>
    /// <param name="pluginManager">插件管理器实例。</param>
    /// <param name="options">配置选项的委托。</param>
    /// <returns>返回<see cref="DmtpRpcFeature"/>实例。</returns>
    public static DmtpRpcFeature UseDmtpRpc(this IPluginManager pluginManager, Action<DmtpRpcOption> options)
    {
        var option = new DmtpRpcOption();

        options.Invoke(option);

        var dmtpRpcFeature = new DmtpRpcFeature(pluginManager.Resolver, option);
        pluginManager.Add(dmtpRpcFeature);

        return dmtpRpcFeature;
    }
    public static DmtpRpcFeature UseDmtpRpc(this IPluginManager pluginManager)
    {
        return UseDmtpRpc(pluginManager, options => { });
    }
}
