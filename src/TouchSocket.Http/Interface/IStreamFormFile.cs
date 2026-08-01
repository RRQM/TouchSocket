//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762606
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Collections.Specialized;

namespace TouchSocket.Http;

/// <summary>
/// 表示通过 <see cref="IStreamingFormReader"/> 读取到的一个表单字段或文件段。
/// </summary>
public interface IStreamFormSection
{
    /// <summary>
    /// 获取 Content-Disposition 头部值
    /// </summary>
    string ContentDisposition { get; }

    /// <summary>
    /// 获取 Content-Type 头部值
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// 获取从头部解析出的所有键值对
    /// </summary>
    NameValueCollection DataPair { get; }

    /// <summary>
    /// 获取文件名（来自 filename 字段）。对于文本字段，此属性为 <see langword="null"/> 或空字符串。
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// 获取字段名（来自 name 字段）
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取当前段是否为文件段
    /// </summary>
    bool IsFile { get; }

    /// <summary>
    /// 异步读取当前段的数据块。返回实际读取字节数，返回 <see langword="0"/> 表示数据已读完。
    /// </summary>
    /// <param name="buffer">接收数据的缓冲区</param>
    /// <param name="cancellationToken">取消令牌</param>
    ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将当前段的数据复制到目标流，不会整体加载到内存。
    /// </summary>
    /// <param name="destination">目标写入流</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将当前段的数据以 UTF-8 字符串形式读取并返回，通常用于文本字段。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default);
}
