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
/// 表示 MCP HTTP 插件配置。
/// </summary>
public sealed class McpHttpPluginOptions
{
    /// <summary>
    /// 获取或设置 MCP 路径。
    /// </summary>
    public string Path { get; set; } = "/mcp";

    /// <summary>
    /// 获取或设置 MCP 服务端协议选项。
    /// </summary>
    public McpServerOptions ServerOptions { get; set; } = new McpServerOptions();

    /// <summary>获取或设置允许的浏览器源。默认拒绝全部携带 Origin 的请求；不携带 Origin 的本地客户端不受此项限制。</summary>
    public ISet<string> AllowedOrigins { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>获取或设置请求授权验证器。返回 false 时拒绝请求；应用可在此验证配对令牌。</summary>
    public Func<TouchSocket.Http.HttpRequest, bool> AuthorizeRequest { get; set; }

    /// <summary>获取或设置最大会话数量，默认 1024。</summary>
    public int MaximumSessions { get; set; } = 1024;

    /// <summary>获取或设置会话空闲超时，默认 30 分钟。执行中的请求不会因空闲超时被回收。</summary>
    public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>获取或设置单次 JSON 请求体的最大字节数，默认 4 MiB。</summary>
    public int MaximumRequestBytes { get; set; } = 4 * 1024 * 1024;
}
