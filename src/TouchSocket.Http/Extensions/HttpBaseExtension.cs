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

using System.Buffers;

namespace TouchSocket.Http;

/// <summary>
/// <see cref="HttpBase"/> 扩展方法。
/// </summary>
public static class HttpBaseExtension
{
    /// <summary>
    /// 异步读取HTTP内容块，返回 <see cref="HttpReadOnlyMemoryBlockResult"/>。
    /// </summary>
    /// <param name="httpBase"><see cref="HttpBase"/> 实例。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>包含本次读取内容及是否完成标志的 <see cref="HttpReadOnlyMemoryBlockResult"/>。</returns>
    public static async ValueTask<HttpReadOnlyMemoryBlockResult> ReadAsync(this HttpBase httpBase, CancellationToken cancellationToken = default)
    {
        var memoryOwner = MemoryPool<byte>.Shared.Rent(81920);
        try
        {
            var read = await httpBase.ReadAsync(memoryOwner.Memory, cancellationToken).ConfigureDefaultAwait();
            if (read == 0)
            {
                memoryOwner.Dispose();
                return HttpReadOnlyMemoryBlockResult.Completed;
            }

            var owner = memoryOwner;
            var memory = memoryOwner.Memory.Slice(0, read);
            return new HttpReadOnlyMemoryBlockResult(owner.Dispose, memory, false);
        }
        catch
        {
            memoryOwner.Dispose();
            throw;
        }
    }
}
