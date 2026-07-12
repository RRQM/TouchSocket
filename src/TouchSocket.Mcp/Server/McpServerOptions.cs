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

using System;
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket.Mcp;

/// <summary>
/// 表示 MCP 服务端配置选项。
/// </summary>
public sealed class McpServerOptions : McpOptionsBase
{
    /// <summary>
    /// 获取或设置服务端支持的 MCP 协议版本列表。
    /// </summary>
    public IList<string> SupportedProtocolVersions { get; set; } = new List<string>
    {
        McpProtocolVersion.Latest
    };

    /// <summary>
    /// 获取或设置服务端实现信息。
    /// </summary>
    public McpImplementationInfo ServerInfo { get; set; } = new McpImplementationInfo
    {
        Name = "TouchSocket.Mcp.Server",
        Version = "1.0.0"
    };

    /// <summary>
    /// 获取或设置服务端的可选指令说明，会在 initialize 响应中返回给客户端。
    /// </summary>
    public string Instructions { get; set; }

    /// <summary>
    /// 协商服务端响应的 MCP 协议版本。
    /// </summary>
    /// <param name="protocolVersion">客户端请求的协议版本。</param>
    /// <returns>服务端响应的协议版本。</returns>
    public string NegotiateProtocolVersion(string protocolVersion)
    {
        if (this.SupportedProtocolVersions == null || this.SupportedProtocolVersions.Count == 0)
        {
            return McpProtocolVersion.Latest;
        }

        if (!string.IsNullOrEmpty(protocolVersion)
            && this.SupportedProtocolVersions.Contains(protocolVersion, StringComparer.Ordinal))
        {
            return protocolVersion;
        }

        return this.SupportedProtocolVersions[0];
    }
}
