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

namespace TouchSocket.Http;

/// <summary>
/// IContentTypeProvider
/// </summary>
public interface IContentTypeProvider
{
    /// <summary>
    /// 给定文件路径，确定MIME类型
    /// </summary>
    /// <param name="subpath">文件路径</param>
    /// <param name="contentType">MIME类型</param>
    /// <returns>如果找到匹配的MIME类型则返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    bool TryGetContentType(string subpath, out string contentType);

    /// <summary>
    /// 添加文件路径和MIME类型的映射
    /// </summary>
    /// <param name="subpath">文件路径</param>
    /// <param name="contentType">MIME类型</param>
    void Add(string subpath, string contentType);
}