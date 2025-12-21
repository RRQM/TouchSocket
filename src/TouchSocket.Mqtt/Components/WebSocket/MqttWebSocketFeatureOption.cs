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

using System;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.Mqtt;

/// <summary>
/// Mqtt WebSocket功能配置选项
/// </summary>
public sealed class MqttWebSocketFeatureOption
{
    /// <summary>
    /// 验证连接的委托方法
    /// </summary>
    public Func<IHttpSessionClient, HttpContext, Task<bool>> VerifyConnection { get; set; }

    /// <summary>
    /// 设置Mqtt WebSocket连接的URL路径
    /// </summary>
    /// <param name="url">Mqtt WebSocket连接路径，如果为null或空则表示所有连接都解释为Mqtt WS</param>
    public void SetUrl(string url = "/mqtt")
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
