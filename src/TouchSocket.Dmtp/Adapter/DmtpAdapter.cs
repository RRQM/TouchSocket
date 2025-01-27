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
using TouchSocket.Core;

namespace TouchSocket.Dmtp;

/// <summary>
/// DmtpAdapter 类，继承自 CustomFixedHeaderByteBlockDataHandlingAdapter&lt;DmtpMessage&gt;
/// 该类用于特定地处理 DmtpMessage，通过自定义的固定头部字节块数据处理适配器实现。
/// </summary>
public class DmtpAdapter : CustomFixedHeaderByteBlockDataHandlingAdapter<DmtpMessage>
{
    /// <inheritdoc/>
    public override bool CanSendRequestInfo => true;

    /// <inheritdoc/>
    public override bool CanSplicingSend => true;

    /// <inheritdoc/>
    public override int HeaderLength => 8;

    /// <summary>
    /// 最大拼接
    /// </summary>
    public const int MaxSplicing = 1024 * 64;

    /// <inheritdoc/>
    protected override DmtpMessage GetInstance()
    {
        return new DmtpMessage();
    }

    /// <inheritdoc/>
    protected override void OnReceivedSuccess(DmtpMessage request)
    {
        request.SafeDispose();
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
    {
        if (requestInfo is not DmtpMessage message)
        {
            throw new Exception($"无法将{nameof(requestInfo)}转换为{nameof(DmtpMessage)}");
        }

        this.ThrowIfMoreThanMaxPackageSize(message.MaxLength);

        using (var byteBlock = new ByteBlock(message.MaxLength))
        {
            var block = byteBlock;
            message.Build(ref block);
            await this.GoSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }
}