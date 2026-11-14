//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//------------------------------------------------------------------------------

namespace TouchSocket.Mcp;

/// <summary>HTTP 会话拥有的协议处理器及活动请求计数。</summary>
internal sealed class McpHttpSession : DisposableObject
{
    internal McpHttpSession(McpActor actor)
    {
        this.Actor = actor;
        this.LastActivityUtc = DateTime.UtcNow;
    }

    internal McpActor Actor { get; }

    internal DateTime LastActivityUtc { get; set; }

    internal int ActiveRequests { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Actor.Dispose();
        }
        base.Dispose(disposing);
    }
}
