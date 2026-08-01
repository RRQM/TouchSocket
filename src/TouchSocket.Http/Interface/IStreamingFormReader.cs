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
/// 流式表单读取器。支持逐段（section）拉取 <c>multipart/form-data</c> 或 <c>application/x-www-form-urlencoded</c> 表单数据，
/// 无论文件大小均不会将请求体整体加载到内存。
/// </summary>
/// <remarks>
/// 典型用法：
/// <code>
/// using var reader = request.GetStreamingFormReader();
/// IStreamFormSection section;
/// while ((section = await reader.ReadNextSectionAsync()) != null)
/// {
///     if (section.IsFile)
///         await section.CopyToAsync(fileStream);
///     else
///         var value = await section.ReadAsStringAsync();
/// }
/// </code>
/// 在调用 <see cref="ReadNextSectionAsync"/> 之前，上一个 <see cref="IStreamFormSection"/> 未读取完毕的数据将被自动丢弃。
/// </remarks>
public interface IStreamingFormReader : IDisposable
{
    /// <summary>
    /// 异步读取下一个表单段。当没有更多段时返回 <see langword="null"/>。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IStreamFormSection> ReadNextSectionAsync(CancellationToken cancellationToken = default);
}
