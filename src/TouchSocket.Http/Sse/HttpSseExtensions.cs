// ------------------------------------------------------------------------------
// 此代码版权(除特别声明或在XREF结尾的命名空间的代码)归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议,若本仓库没有设置,则按MIT开源协议授权
// CSDN博客:https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频:https://space.bilibili.com/94253567
// Gitee源代码仓库:https://gitee.com/RRQM_Home
// Github源代码仓库:https://github.com/RRQM
// API首页:https://touchsocket.net/
// 交流QQ群:234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;

namespace TouchSocket.Http;

/// <summary>
/// HTTP SSE扩展方法
/// </summary>
public static class HttpSseExtensions
{
    /// <summary>
    /// 创建SSE读取器
    /// </summary>
    /// <param name="response">HTTP响应</param>
    /// <returns>SSE读取器</returns>
    public static SseReader CreateSseReader(this HttpResponse response)
    {
        var contentType = response.ContentType;
        if (contentType.IsEmpty || !contentType.First.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("响应的Content-Type不是text/event-stream");
        }

        return new InternalSseReader(response);
    }

    /// <summary>
    /// 创建SSE写入器
    /// </summary>
    /// <param name="response">HTTP响应</param>
    /// <returns>SSE写入器</returns>
    public static SseWriter CreateSseWriter(this HttpResponse response)
    {
        response.ContentType = "text/event-stream";
        response.Headers.TryAdd(HttpHeaders.CacheControl, "no-cache");
        response.Headers.TryAdd(HttpHeaders.Connection, "keep-alive");

        return new InternalSseWriter(response);
    }
}
