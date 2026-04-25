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

namespace TouchSocket.WebApi;

/// <summary>
/// 用于显式声明 WebApi 方法在 OpenAPI 文档中的响应类型。
/// 当通过 AOP/过滤器修改了方法的实际返回值类型时，可使用该特性保持 OpenAPI 文档与运行时行为一致。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class WebApiProducesResponseTypeAttribute : Attribute
{
    /// <summary>
    /// 使用指定的响应类型和状态码初始化 <see cref="WebApiProducesResponseTypeAttribute"/>。
    /// </summary>
    /// <param name="type">该状态码对应的响应体类型。</param>
    /// <param name="statusCode">HTTP 状态码，默认为 200。</param>
    public WebApiProducesResponseTypeAttribute(Type type, int statusCode = 200)
    {
        ThrowHelper.ThrowIfNull(type, nameof(type));
        this.Type = type;
        this.StatusCode = statusCode;
    }

    /// <summary>
    /// 响应体类型。
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// HTTP 状态码。
    /// </summary>
    public int StatusCode { get; }
}
