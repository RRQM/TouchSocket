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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket功能配置选项
/// </summary>
public sealed class WebSocketFeatureOptions
{
    /// <summary>
    /// 是否自动处理Close报文，默认为true
    /// </summary>
    public bool AutoClose { get; set; } = true;

    /// <summary>
    /// 当收到ping报文时，是否自动回应pong，默认为false
    /// </summary>
    public bool AutoPong { get; set; } = false;

    /// <summary>
    /// 验证连接的委托方法
    /// </summary>
    public Func<IHttpSessionClient, HttpContext, Task<bool>> VerifyConnection { get; set; }

    /// <summary>
    /// 设置是否自动处理Close报文
    /// </summary>
    /// <param name="autoClose">是否自动处理Close报文</param>
    public void SetAutoClose(bool autoClose)
    {
        this.AutoClose = autoClose;
    }

    /// <summary>
    /// 设置是否自动回应Ping报文
    /// </summary>
    /// <param name="autoPong">是否自动回应Ping报文</param>
    public void SetAutoPong(bool autoPong)
    {
        this.AutoPong = autoPong;
    }

    /// <summary>
    /// 设置WebSocket连接的URL路径
    /// </summary>
    /// <param name="url">WebSocket连接路径，如果为null或空则表示所有连接都解释为WS</param>
    public void SetUrl(string url = "/ws")
    {
        if (url.IsNullOrEmpty())
        {
            url = "/";
        }
        else if (!url.StartsWith("/"))
        {
            url = "/" + url;
        }

        this.SetVerifyConnection((client, context) =>
        {
            return url == "/" || context.Request.UrlEquals(url);
        });
    }

    /// <summary>
    /// 设置验证连接的同步方法
    /// </summary>
    /// <param name="verifyConnection">验证连接的同步委托</param>
    public void SetVerifyConnection(Func<IHttpSessionClient, HttpContext, bool> verifyConnection)
    {
        ThrowHelper.ThrowIfNull(verifyConnection, nameof(verifyConnection));

        this.VerifyConnection = (client, context) =>
        {
            return Task.FromResult(verifyConnection.Invoke(client, context));
        };
    }

    /// <summary>
    /// 设置验证连接的异步方法
    /// </summary>
    /// <param name="verifyConnection">验证连接的异步委托</param>
    public void SetVerifyConnection(Func<IHttpSessionClient, HttpContext, Task<bool>> verifyConnection)
    {
        ThrowHelper.ThrowIfNull(verifyConnection, nameof(verifyConnection));
        this.VerifyConnection = verifyConnection;
    }
}