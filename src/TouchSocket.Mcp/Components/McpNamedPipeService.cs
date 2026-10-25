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

using TouchSocket.NamedPipe;
using TouchSocket.Rpc;

namespace TouchSocket.Mcp;

/// <summary>
/// 基于命名管道的 MCP 服务端，使用换行符分隔 UTF-8 JSON 消息。
/// </summary>
public class McpNamedPipeService : NamedPipeService<McpNamedPipeSessionClient>
{
    /// <inheritdoc/>
    protected override McpNamedPipeSessionClient NewClient()
    {
        return new McpNamedPipeSessionClient();
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);
        var options = config.GetValue(McpConfigExtension.McpServerOptionsProperty);
        if (options?.JsonSerializerOptions.WriteIndented == true)
        {
            throw new InvalidOperationException("MCP named pipe messages require compact JSON. WriteIndented must be false.");
        }
        if (this.Resolver.Resolve<IRpcServerProvider>() == null)
        {
            throw new InvalidOperationException("IRpcServerProvider is required for McpNamedPipeService.");
        }
    }
}
