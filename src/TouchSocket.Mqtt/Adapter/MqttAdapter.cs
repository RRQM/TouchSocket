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

namespace TouchSocket.Mqtt;

internal class MqttAdapter : CustomDataHandlingAdapter<MqttMessage>
{
    public override bool CanSendRequestInfo => true;

    public MqttProtocolVersion Version { get; private set; } = MqttProtocolVersion.Unknown;

    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref MqttMessage request, ref int tempCapacity)
    {
        if (byteBlock.CanReadLength < 2)
        {
            return FilterResult.Cache;
        }
        var position = byteBlock.Position;
        var firstByte = byteBlock.ReadByte();
        var mqttControlPacketType = (MqttMessageType)firstByte.GetHeight4();
        //var fixHeaderFlags = new FixHeaderFlags(firstByte);

        var remainingLength = MqttExtension.ReadVariableByteInteger(ref byteBlock);
        if (byteBlock.CanReadLength < remainingLength)
        {
            byteBlock.Position = position;
            return FilterResult.Cache;
        }

        var mqttMessage = MqttMessage.CreateMqttMessage(mqttControlPacketType);

        //Console.WriteLine("Rev:"+ mqttMessage.MessageType);

        if (mqttMessage is not MqttConnectMessage)
        {
            mqttMessage.InternalSetVersion(this.Version);
        }

        byteBlock.Position = position;
        mqttMessage.Unpack(ref byteBlock);
        if (byteBlock.Position != position + remainingLength + 2)
        {
            throw new Exception("存在没有读取的数据");
        }

        if (mqttMessage is MqttConnectMessage connectMessage)
        {
            this.Version = connectMessage.Version;
        }

        request = mqttMessage;
        return FilterResult.Success;
    }

    protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
    {
        if (requestInfo is MqttMessage mqttMessage)
        {
            //Console.WriteLine("Send:" + mqttMessage.MessageType);

            if (mqttMessage.MessageType == MqttMessageType.Connect)
            {
                this.Version = ((MqttConnectMessage)mqttMessage).Version;
            }
            var byteBlock = new ValueByteBlock(mqttMessage.MaxLength);
            try
            {
                mqttMessage.InternalSetVersion(this.Version);
                mqttMessage.Build(ref byteBlock);
                await this.GoSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}