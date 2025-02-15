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

using System.Collections.Generic;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi;

/// <summary>
/// Web API 映射接口。
/// </summary>
public interface IWebApiMapping : IEnumerable<MappingMethod>
{
    /// <summary>
    /// 将映射设置为只读。
    /// </summary>
    void MakeReadonly();

    /// <summary>
    /// 根据指定的 URL 和 HTTP 方法匹配 RPC 方法。
    /// </summary>
    /// <param name="url">要匹配的 URL。</param>
    /// <param name="httpMethod">要匹配的 HTTP 方法。</param>
    /// <returns>匹配的 RPC 方法。</returns>
    RpcMethod Match(string url, HttpMethod httpMethod);
}