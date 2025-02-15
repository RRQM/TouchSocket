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

using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi;

/// <summary>
/// 表示Web API的映射方法。
/// </summary>
public readonly struct MappingMethod
{
    /// <summary>
    /// 初始化<see cref="MappingMethod"/>结构的新实例。
    /// </summary>
    /// <param name="url">映射方法的URL。</param>
    /// <param name="httpMethod">映射方法的HTTP方法。</param>
    /// <param name="rpcMethod">映射方法的RPC方法。</param>
    /// <param name="isRegex">指示URL是否为正则表达式。</param>
    public MappingMethod(string url, HttpMethod httpMethod, RpcMethod rpcMethod, bool isRegex)
    {
        this.Url = url;
        this.HttpMethod = httpMethod;
        this.RpcMethod = rpcMethod;
        this.IsRegex = isRegex;
    }

    /// <summary>
    /// 获取映射方法的URL。
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// 获取映射方法的HTTP方法。
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    /// 获取映射方法的RPC方法。
    /// </summary>
    public RpcMethod RpcMethod { get; }

    /// <summary>
    /// 获取一个值，该值指示URL是否为正则表达式。
    /// </summary>
    public bool IsRegex { get; }
}