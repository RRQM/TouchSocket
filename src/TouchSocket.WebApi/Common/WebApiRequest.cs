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
/// 表示一个 Web API 请求。
/// </summary>
public class WebApiRequest
{
    /// <summary>
    /// 获取或设置 HTTP 方法类型。
    /// </summary>
    public HttpMethodType Method { get; set; }

    /// <summary>
    /// 获取或设置请求的主体。
    /// </summary>
    public object Body { get; set; }

    /// <summary>
    /// 获取或设置请求的内容类型。
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// 获取或设置请求的头部信息。
    /// </summary>
    public KeyValuePair<string, string>[] Headers { get; set; }

    /// <summary>
    /// 获取或设置请求的查询参数。
    /// </summary>
    public KeyValuePair<string, string>[] Querys { get; set; }

    /// <summary>
    /// 获取或设置请求的表单数据。
    /// </summary>
    public KeyValuePair<string, string>[] Forms { get; set; }
}