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

using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 定义一个静态类SocketPluginManagerExtension，用于扩展Socket插件管理功能
/// </summary>
public static class SocketPluginManagerExtension
{
    /// <summary>
    ///  检查连接客户端活性插件。
    ///  <para>当在设置的周期内，没有接收/发送任何数据，则判定该客户端掉线。执行清理。默认配置：60秒为一个周期，同时检测发送和接收。</para>
    ///  服务器、客户端均适用。
    /// </summary>
    /// <param name="pluginManager">插件管理器对象，用于管理插件。</param>
    /// <returns>返回一个<see cref="CheckClearPlugin{TClient}"/>类型的插件实例，用于执行客户端活性检查及清理操作。</returns>
    [Obsolete("此方法由于不能很好的描述CheckClear的应用对象，已被弃用，请直接使用UseTcpSessionCheckClear代替")]
    public static CheckClearPlugin<ITcpSession> UseCheckClear(this IPluginManager pluginManager)
    {
        // 使用插件管理器添加一个检查和清理不活跃客户端的插件
        return pluginManager.Add<CheckClearPlugin<ITcpSession>>();
    }

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
    public static CheckClearPlugin<TClient> UseCheckClear<TClient>(this IPluginManager pluginManager) where TClient : class, IDependencyClient, IClosableClient,IOnlineClient
    {
        // 添加并返回一个新的检查和清理插件实例
        return pluginManager.Add<CheckClearPlugin<TClient>>();
    }

    #region Reconnection

    /// <summary>
    /// 使用断线重连。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="pluginManager"></param>
    /// <returns></returns>
    [Obsolete("此配置已被弃用，请使用UseTcpReconnection代替", true)]
    public static ReconnectionPlugin<TClient> UseReconnection<TClient>(this IPluginManager pluginManager) where TClient : class, ITcpClient
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 使用断线重连。
    /// <para>该效果仅客户端在完成首次连接，且为被动断开时有效。</para>
    /// </summary>
    /// <param name="pluginManager"></param>
    /// <param name="successCallback">成功回调函数</param>
    /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
    /// <param name="printLog">是否输出日志。</param>
    /// <param name="sleepTime">失败时，停留时间</param>
    /// <returns></returns>
    [Obsolete("此配置已被弃用，请使用UseTcpReconnection代替", true)]
    public static ReconnectionPlugin<ITcpClient> UseReconnection(this IPluginManager pluginManager, int tryCount = 10, bool printLog = false, int sleepTime = 1000, Action<ITcpClient> successCallback = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 使用断线重连。
    /// <para>该效果仅客户端在完成首次连接，且为被动断开时有效。</para>
    /// </summary>
    /// <param name="pluginManager"></param>
    /// <param name="sleepTime">失败时间隔时间</param>
    /// <param name="failCallback">失败时回调（参数依次为：客户端，本轮尝试重连次数，异常信息）。如果回调为null或者返回false，则终止尝试下次连接。</param>
    /// <param name="successCallback">成功连接时回调。</param>
    /// <returns></returns>
    [Obsolete("此配置已被弃用，请使用UseTcpReconnection代替", true)]
    public static ReconnectionPlugin<ITcpClient> UseReconnection(this IPluginManager pluginManager, TimeSpan sleepTime,
        Func<ITcpClient, int, Exception, bool> failCallback = default,
        Action<ITcpClient> successCallback = default)
    {
        throw new NotImplementedException();
    }

    #endregion Reconnection

    #region TcpReconnection

    /// <summary>
    /// 使用断线重连。
    /// </summary>
    /// <typeparam name="TClient">指定的客户端类型，必须继承自ITcpClient。</typeparam>
    /// <param name="pluginManager">插件管理器实例，用于添加断线重连插件。</param>
    /// <returns>返回创建的重连实例。</returns>
    public static ReconnectionPlugin<TClient> UseTcpReconnection<TClient>(this IPluginManager pluginManager) where TClient : ITcpClient
    {
        // 创建并初始化断线重连插件实例
        var reconnectionPlugin = new TcpReconnectionPlugin<TClient>();
        // 将断线重连插件添加到插件管理器中
        pluginManager.Add(reconnectionPlugin);
        // 返回断线重连插件实例
        return reconnectionPlugin;
    }

    /// <summary>
    /// 为插件管理器添加TCP重新连接插件。
    /// </summary>
    /// <param name="pluginManager">要添加插件的插件管理器。</param>
    /// <returns>返回新创建的TCP重新连接插件实例。</returns>
    public static ReconnectionPlugin<ITcpClient> UseTcpReconnection(this IPluginManager pluginManager)
    {
        // 创建TCP重新连接插件实例
        var reconnectionPlugin = new TcpReconnectionPlugin<ITcpClient>();
        // 将插件添加到插件管理器中
        pluginManager.Add(reconnectionPlugin);
        // 返回新创建的插件实例
        return reconnectionPlugin;
    }
    #endregion TcpReconnection
}