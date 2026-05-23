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
/// 提供 MCP 组件配置扩展。
/// </summary>
public static class McpConfigExtension
{
    /// <summary>
    /// MCP 客户端协议配置属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<McpClientOptions> McpClientOptionsProperty = new("McpClientOptions", null);

    /// <summary>
    /// MCP stdio 客户端配置属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<McpStdioClientOptions> McpStdioClientOptionsProperty = new("McpStdioClientOptions", null);

    /// <summary>
    /// MCP stdio 服务端配置属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<McpStdioServerOptions> McpStdioServerOptionsProperty = new("McpStdioServerOptions", null);

    /// <summary>
    /// MCP HTTP 插件配置属性。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(TouchSocketConfig), ActionMode = true)]
    public static readonly DependencyProperty<McpHttpPluginOptions> McpHttpPluginOptionsProperty = new("McpHttpPluginOptions", null);
}
