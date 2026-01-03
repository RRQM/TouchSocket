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

namespace TouchSocket.Http;

public abstract class SseReader
{
    public abstract HttpResponse Response { get; }

    /// <summary>
    /// 异步读取下一个SSE消息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SSE消息,如果流结束返回<see langword="null"/></returns>
    public abstract Task<SseMessage> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步读取所有SSE消息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SSE消息的异步枚举</returns>
    public abstract IAsyncEnumerable<SseMessage> ReadAllAsync(CancellationToken cancellationToken = default);
}