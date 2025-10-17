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

namespace TouchSocket.Sockets;

/// <summary>
/// 定义一个静态类SocketPluginManagerExtension，用于扩展Socket插件管理功能
/// </summary>
public static class SocketPluginManagerExtension
{
    /// <summary>
    ///  使用<see cref="ITcpSession"/>检查连接客户端活性插件。
    ///  <para>当在设置的周期内，没有接收/发送任何数据，则判定该客户端掉线。执行清理。默认配置：60秒为一个周期，同时检测发送和接收。</para>
    ///  服务器、客户端均适用。
    /// </summary>
    /// <param name="pluginManager">插件管理器对象，用于管理插件。</param>
    /// <returns>返回一个<see cref="CheckClearPlugin{TClient}"/>类型的插件实例，用于执行客户端活性检查及清理操作。</returns>
    public static CheckClearPlugin<ITcpSession> UseTcpSessionCheckClear(this IPluginManager pluginManager)
    {
        return pluginManager.UseCheckClear<ITcpSession>();
    }

    /// <summary>
    ///  检查连接客户端活性插件。
    ///  <para>当在设置的周期内，没有接收/发送任何数据，则判定该客户端掉线。执行清理。默认配置：60秒为一个周期，同时检测发送和接收。</para>
    ///  服务器、客户端均适用。
    /// </summary>
    /// <param name="pluginManager">插件管理器</param>
    /// <returns>返回一个用于检查和清理不活跃客户端的插件实例</returns>
    public static CheckClearPlugin<TClient> UseCheckClear<TClient>(this IPluginManager pluginManager)
        where TClient : class, IDependencyClient, IClosableClient
    {
        // 添加并返回一个新的检查和清理插件实例
        return pluginManager.Add<CheckClearPlugin<TClient>>();
    }

    #region Reconnection

    /// <summary>
    /// 使用断线重连
    /// </summary>
    /// <typeparam name="TClient">客户端类型,必须实现<see cref="IConnectableClient"/>,<see cref="IOnlineClient"/>和<see cref="IDependencyClient"/>接口</typeparam>
    /// <param name="pluginManager">插件管理器实例</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>返回创建的重连插件实例</returns>
    public static ReconnectionPlugin<TClient> UseReconnection<TClient>(
        this IPluginManager pluginManager,
        Action<ReconnectionOptions<TClient>>? configureOptions = null)
        where TClient : IConnectableClient, IOnlineClient, IDependencyClient
    {
        var options = new ReconnectionOptions<TClient>();
        configureOptions?.Invoke(options);
        
        var reconnectionPlugin = new ReconnectionPlugin<TClient>(options);
        pluginManager.Add(reconnectionPlugin);
        return reconnectionPlugin;
    }

    #endregion Reconnection
}