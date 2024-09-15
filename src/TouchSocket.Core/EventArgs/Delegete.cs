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

namespace TouchSocket.Core
{
    /// <summary>
    /// 插件泛型基础事件委托
    /// </summary>
    /// <typeparam name="TClient">客户端类型，表示事件发生的上下文环境</typeparam>
    /// <typeparam name="TEventArgs">事件参数类型，必须继承自PluginEventArgs</typeparam>
    /// <param name="client">触发事件的客户端实例</param>
    /// <param name="e">包含事件数据的事件参数</param>
    public delegate void PluginEventHandler<TClient, TEventArgs>(TClient client, TEventArgs e) where TEventArgs : PluginEventArgs;

    /// <summary>
    /// TouchSocket基础泛型事件委托
    /// </summary>
    /// <typeparam name="TClient">触发事件的客户端类型</typeparam>
    /// <typeparam name="TEventArgs">事件参数类型</typeparam>
    /// <param name="client">触发事件的客户端实例</param>
    /// <param name="e">事件参数</param>
    public delegate void TouchSocketEventHandler<TClient, TEventArgs>(TClient client, TEventArgs e) where TEventArgs : TouchSocketEventArgs;
}