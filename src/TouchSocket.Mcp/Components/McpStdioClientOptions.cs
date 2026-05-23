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

using System.Diagnostics;

namespace TouchSocket.Mcp;

/// <summary>
/// 表示 MCP stdio 客户端传输配置。
/// </summary>
public sealed class McpStdioClientOptions
{
    /// <summary>
    /// 获取或设置子进程启动信息。
    /// </summary>
    public ProcessStartInfo StartInfo { get; set; }

    /// <summary>
    /// 获取或设置已存在的进程实例。
    /// </summary>
    public Process Process { get; set; }

    /// <summary>
    /// 获取或设置释放时是否终止进程。
    /// </summary>
    public bool KillOnDispose { get; set; } = true;

    /// <summary>
    /// 获取或设置 MCP 客户端协议选项。
    /// </summary>
    public McpClientOptions ClientOptions { get; set; } = new McpClientOptions();
}
