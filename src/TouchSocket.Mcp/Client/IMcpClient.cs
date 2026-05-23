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

namespace TouchSocket.Mcp;

/// <summary>
/// 定义 MCP 客户端的核心操作接口。
/// </summary>
public interface IMcpClient
{
    /// <summary>
    /// 执行 initialize 握手，与服务端协商协议版本和能力。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>服务端返回的初始化结果。</returns>
    Task<McpInitializeResult> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务端所有可用工具列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>工具列表结果。</returns>
    Task<McpListToolsResult> ListToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 调用服务端指定工具。
    /// </summary>
    /// <param name="name">工具名称。</param>
    /// <param name="arguments">工具调用参数字典，键为参数名，值为参数值。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>工具调用结果。</returns>
    Task<McpCallToolResult> CallToolAsync(string name, Dictionary<string, object> arguments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务端所有可用资源列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>资源列表结果。</returns>
    Task<McpListResourcesResult> ListResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务端所有资源模板列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>资源模板列表结果。</returns>
    Task<McpListResourceTemplatesResult> ListResourceTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取指定 URI 的资源内容。
    /// </summary>
    /// <param name="uri">资源 URI。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>资源读取结果。</returns>
    Task<McpReadResourceResult> ReadResourceAsync(string uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务端所有可用提示模板列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>提示模板列表结果。</returns>
    Task<McpListPromptsResult> ListPromptsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定提示模板的内容。
    /// </summary>
    /// <param name="name">提示模板名称。</param>
    /// <param name="arguments">参数字典，键为参数名，值为字符串值。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>提示模板获取结果。</returns>
    Task<McpGetPromptResult> GetPromptAsync(string name, Dictionary<string, string> arguments = null, CancellationToken cancellationToken = default);
}
