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
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 固定长度包适配器。
/// 用于处理每个数据包长度固定的场景。
/// </summary>
public class FixedSizePackageAdapter : SingleStreamDataHandlingAdapter
{
    /// <summary>
    /// 构造函数，指定固定包长度。
    /// </summary>
    /// <param name="fixedSize">每个包的固定长度。</param>
    public FixedSizePackageAdapter(int fixedSize)
    {
        this.FixedSize = fixedSize;
    }

    /// <summary>
    /// 获取固定包长度。
    /// </summary>
    public int FixedSize { get; private set; }

    /// <summary>
    /// 预处理接收到的数据。
    /// </summary>
    /// <param name="reader">数据读取器。</param>
    protected override async Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            if (reader.BytesRemaining < this.FixedSize)
            {
                return;
            }

            var memory = reader.GetMemory(this.FixedSize);
            await this.GoReceivedAsync(memory, null).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            reader.Advance(this.FixedSize);
        }
    }

    public override void SendInput<TWriter>(ref TWriter writer, in ReadOnlyMemory<byte> memory)
    {
        var dataLen = memory.Length;
        if (dataLen != this.FixedSize)
        {
            throw new OverlengthException(TouchSocketCoreResource.ValueMoreThan.Format(nameof(memory.Length), this.FixedSize));
        }
        base.SendInput(ref writer, memory);
    }
}