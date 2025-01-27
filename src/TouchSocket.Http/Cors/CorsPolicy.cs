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

using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// CorsResult
/// </summary>
public class CorsPolicy
{
    /// <summary>
    /// CorsResult
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="headers"></param>
    /// <param name="methods"></param>
    /// <param name="origin"></param>
    public CorsPolicy(bool credentials, string headers, string methods, string origin)
    {
        this.Credentials = credentials;
        this.Headers = headers;
        this.Methods = methods;
        this.Origin = origin;
    }

    /// <summary>
    /// 允许客户端携带验证信息
    /// </summary>
    public bool Credentials { get; }

    /// <summary>
    /// 请求头
    /// </summary>
    public string Headers { get; }

    /// <summary>
    /// 允许跨域的方法。
    /// </summary>
    public string Methods { get; }

    /// <summary>
    /// 允许跨域的域名
    /// </summary>
    public string Origin { get; }

    /// <summary>
    /// 应用跨域策略
    /// </summary>
    /// <param name="context"></param>
    public void Apply(HttpContext context)
    {
        if (this.Origin.HasValue())
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", this.Origin);
        }

        if (this.Credentials)
        {
            context.Response.Headers.Add("Access-Control-Allow-Credentials", this.Credentials.ToString().ToLower());
        }

        if (this.Headers.HasValue())
        {
            context.Response.Headers.Add("Access-Control-Allow-Headers", this.Headers);
        }

        if (this.Methods.HasValue())
        {
            context.Response.Headers.Add("Access-Control-Allow-Methods", this.Methods);
        }
    }
}