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
using TouchSocket.Http.WebSockets;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 基于WebSocket协议的JsonRpc配置选项
/// </summary>
public class WebSocketJsonRpcOption : JsonRpcOption
{
    /// <summary>
    /// 经过判断是否标识当前的客户端为JsonRpc
    /// </summary>
    public Func<IWebSocket, HttpContext, Task<bool>> AllowJsonRpc { get; set; }

    /// <summary>
    /// 设置是否允许JsonRpc的判断逻辑
    /// </summary>
    /// <param name="allowJsonRpc">判断逻辑</param>
    /// <returns>返回当前<see cref="WebSocketJsonRpcOption"/>实例，支持链式调用</returns>
    public WebSocketJsonRpcOption SetAllowJsonRpc(Func<IWebSocket, HttpContext, Task<bool>> allowJsonRpc)
    {
        this.AllowJsonRpc = allowJsonRpc;
        return this;
    }

    /// <summary>
    /// 设置是否允许JsonRpc的判断逻辑
    /// </summary>
    /// <param name="allowJsonRpc">判断逻辑</param>
    /// <returns>返回当前<see cref="WebSocketJsonRpcOption"/>实例，支持链式调用</returns>
    public WebSocketJsonRpcOption SetAllowJsonRpc(Func<IWebSocket, HttpContext, bool> allowJsonRpc)
    {
        this.AllowJsonRpc = (client, context) => Task.FromResult(allowJsonRpc(client, context));
        return this;
    }
}