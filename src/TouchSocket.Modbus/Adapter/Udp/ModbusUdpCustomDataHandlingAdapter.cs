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

using System.Net;

namespace TouchSocket.Modbus;

internal abstract class ModbusUdpCustomDataHandlingAdapter<TRequest> : UdpDataHandlingAdapter
    where TRequest : IRequestInfo
{
    public override bool CanSendRequestInfo => true;

    protected abstract FilterResult Filter<TReader>(ref TReader reader, ref TRequest request)
        where TReader : IBytesReader;

    protected override async Task PreviewReceivedAsync(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory)
    {
        var reader = new BytesReader(memory);
        TRequest request = default;

        if (this.Filter(ref reader, ref request) == FilterResult.Success && reader.BytesRemaining == 0)
        {
            await this.GoReceived(remoteEndPoint, default, request).ConfigureDefaultAwait();
        }
    }

    protected override Task PreviewSendAsync(EndPoint endPoint, IRequestInfo requestInfo, CancellationToken cancellationToken)
    {
        if (requestInfo is IBytesBuilder builder)
        {
            return this.GoSendAsync(endPoint, builder.BuildAsBytes(), cancellationToken);
        }

        throw new NotSupportedException($"不支持发送类型为{nameof(IBytesBuilder)}以外的请求。");
    }
}