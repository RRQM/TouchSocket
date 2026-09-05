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

using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TouchSocket.Mcp;

/// <summary>
/// 将 MCP 工具定义包装为 <see cref="AIFunction"/>。
/// </summary>
public sealed class McpAIFunction : AIFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IMcpClient m_client;
    private readonly McpToolDefinition m_tool;
    private readonly JsonElement m_jsonSchema;
    private readonly JsonElement? m_returnJsonSchema;

    /// <summary>
    /// 初始化 <see cref="McpAIFunction"/> 的新实例。
    /// </summary>
    /// <param name="client">MCP 客户端。</param>
    /// <param name="tool">MCP 工具定义。</param>
    public McpAIFunction(IMcpClient client, McpToolDefinition tool)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (tool == null)
        {
            throw new ArgumentNullException(nameof(tool));
        }

        this.m_client = client;
        this.m_tool = tool;
        this.m_jsonSchema = CreateJsonElement(tool.InputSchema ?? new McpToolInputSchema());
        this.m_returnJsonSchema = tool.OutputSchema == null
            ? null
            : CreateJsonElement(tool.OutputSchema);
    }

    /// <inheritdoc/>
    public override string Name => this.m_tool.Name ?? string.Empty;

    /// <inheritdoc/>
    public override string Description => this.m_tool.Description ?? string.Empty;

    /// <inheritdoc/>
    public override JsonElement JsonSchema => this.m_jsonSchema;

    /// <inheritdoc/>
    public override JsonElement? ReturnJsonSchema => this.m_returnJsonSchema;

    /// <inheritdoc/>
    public override JsonSerializerOptions JsonSerializerOptions => JsonOptions;

    /// <inheritdoc/>
    protected override async ValueTask<object> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var mcpArguments = CreateMcpArguments(arguments);
        var result = await this.m_client.CallToolAsync(this.Name, mcpArguments, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.SerializeToElement(result, JsonOptions);
    }

    private static Dictionary<string, object> CreateMcpArguments(AIFunctionArguments arguments)
    {
        var mcpArguments = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var argument in arguments)
        {
            mcpArguments[argument.Key] = argument.Value;
        }

        return mcpArguments;
    }

    private static JsonElement CreateJsonElement<T>(T value)
    {
        return JsonSerializer.SerializeToElement(value, JsonOptions);
    }
}
