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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// 只读内存级别的Http内容。
/// </summary>
public class ReadonlyMemoryHttpContent : HttpContent
{
    private readonly ReadOnlyMemory<byte> m_memory;

    /// <summary>
    /// 初始化 <see cref="ReadonlyMemoryHttpContent"/> 类的新实例。
    /// </summary>
    /// <param name="memory">要封装的只读内存。</param>
    /// <param name="contentType">内容类型</param>
    public ReadonlyMemoryHttpContent(ReadOnlyMemory<byte> memory, string contentType = default)
    {
        this.m_memory = memory;
        this.m_contentType = contentType;
    }

    private readonly string m_contentType;

    /// <summary>
    /// 获取封装的只读内存。
    /// </summary>
    public ReadOnlyMemory<byte> Memory => this.m_memory;

    /// <inheritdoc/>
    protected override bool OnBuildingContent<TByteBlock>(ref TByteBlock byteBlock)
    {
        if (this.m_memory.IsEmpty)
        {
            return true;//直接构建成功，也不用调用后续的WriteContent
        }
        if (byteBlock.FreeLength > this.m_memory.Length)
        {
            //如果空闲空间足够，构建成功，也不用调用后续的WriteContent
            byteBlock.Write(this.m_memory.Span);
            return true;
        }

        //返回<see langword="false"/>，提示后续数据可能太大，通过WriteContent执行。
        return false;
    }

    /// <inheritdoc/>
    protected override void OnBuildingHeader(IHttpHeader header)
    {
        header.Add(HttpHeaders.ContentLength, this.m_memory.Length.ToString());
        if (this.m_contentType != null)
        {
            header.Add(HttpHeaders.ContentType, this.m_contentType);
        }
        else
        {
            header.TryAdd(HttpHeaders.ContentType, "application/octet-stream");
        }
    }

    /// <inheritdoc/>
    protected override async Task WriteContent(Func<ReadOnlyMemory<byte>, Task> writeFunc, CancellationToken token)
    {
        await writeFunc(this.m_memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = this.m_memory.Length;
        return true;
    }
}