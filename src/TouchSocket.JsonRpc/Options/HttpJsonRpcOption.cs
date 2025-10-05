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
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 基于Http协议的JsonRpc配置选项
/// </summary>
public class HttpJsonRpcOption : JsonRpcOption
{
    /// <summary>
    /// 允许JsonRpc的委托。
    /// </summary>
    public Func<IHttpSessionClient, HttpContext, Task<bool>> AllowJsonRpc { get; set; } = (client, context) => Task.FromResult(false);

    /// <summary>
    /// 设置允许JsonRpc的委托。
    /// </summary>
    /// <param name="allowJsonRpc">允许JsonRpc的委托</param>
    public void SetAllowJsonRpc(Func<IHttpSessionClient, HttpContext, Task<bool>> allowJsonRpc)
    {
        this.AllowJsonRpc = allowJsonRpc;
    }

    /// <summary>
    /// 设置允许JsonRpc的Url。
    /// </summary>
    /// <param name="url">允许的Url，默认为"/jsonrpc"。</param>
    public void SetAllowJsonRpc(string url = "/jsonrpc")
    {
        this.AllowJsonRpc = (client, context) => Task.FromResult(context.Request.UrlEquals(url));
    }

    /// <summary>
    /// 设置允许JsonRpc的委托。
    /// </summary>
    /// <param name="allowJsonRpc">允许JsonRpc的委托</param>
    public void SetAllowJsonRpc(Func<IHttpSessionClient, HttpContext, bool> allowJsonRpc)
    {
        this.AllowJsonRpc = (client, context) => Task.FromResult(allowJsonRpc(client, context));
    }
}