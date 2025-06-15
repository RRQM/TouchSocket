// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;
using TouchSocket.Core;

namespace TouchSocket.Http;
class HttpReadOnlyMemoryBlockResult : IReadOnlyMemoryBlockResult
{

    public static readonly IReadOnlyMemoryBlockResult Completed = new HttpReadOnlyMemoryBlockResult(() => { }) { IsCompleted = true };

    public static IReadOnlyMemoryBlockResult FromResult(ReadOnlyMemory<byte> memory)
    {
        return new HttpReadOnlyMemoryBlockResult(() => { }) { IsCompleted = true, Memory = memory };
    }

    private readonly Action m_actionForDispose;

    public HttpReadOnlyMemoryBlockResult(Action actionForDispose)
    {
        this.m_actionForDispose = actionForDispose;
    }
    public ReadOnlyMemory<byte> Memory { get; set; }

    public bool IsCompleted { get; set; }

    public string Message { get; set; }

    public void Dispose()
    {
        this.m_actionForDispose.Invoke();
    }
}