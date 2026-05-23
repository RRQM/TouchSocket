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

using System.Reflection;
using TouchSocket.Rpc;

namespace TouchSocket.Mcp;

/// <summary>
/// 标记一个方法作为 MCP 资源提供者，通过 URI 模板匹配资源读取请求。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class McpResourceAttribute : RpcAttribute
{
    /// <summary>
    /// 使用指定 URI 模板初始化 <see cref="McpResourceAttribute"/>。
    /// </summary>
    /// <param name="uriPattern">资源 URI 模式（精确匹配或前缀匹配）。</param>
    public McpResourceAttribute(string uriPattern)
    {
        this.m_uriPattern = uriPattern;
    }

    private readonly string m_uriPattern;

    /// <summary>
    /// 获取资源 URI 模式。
    /// </summary>
    public string UriPattern => this.m_uriPattern;

    /// <summary>
    /// 获取或设置资源 MIME 类型。
    /// </summary>
    public string MimeType { get; set; }

    /// <inheritdoc/>
    protected override PropertyInfo[] GetPublicProperties()
    {
        return typeof(McpResourceAttribute).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }
}