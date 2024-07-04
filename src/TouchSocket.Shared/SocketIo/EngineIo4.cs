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
using System.Linq;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.SocketIo
{
    internal class EngineIo4 : IEngineIo
    {
        public EngineIo4(EngineIoTransportType engineIOTransportType)
        {
            this.EngineIoTransportType = engineIOTransportType;
        }

        public EngineIoTransportType EngineIoTransportType { get; private set; }

        #region Encode

        public void EncodeToBinary(EngineIoMessage message, ByteBlock byteBlock)
        {
            byteBlock.WriteByte((byte)message.MessageType);
            byteBlock.Write(message.GetRawData());
        }

        public string EncodeToString(EngineIoMessage message)
        {
            var builder = new StringBuilder();
            builder.Append(message.IsText ? ((int)message.MessageType).ToString() : "b");
            builder.Append(message.IsText ? message.GetText() : Convert.ToBase64String(message.GetRawData()));

            return builder.ToString();
        }

        #endregion Encode

        #region Decode

        public const string Seperator = "\u001e";

        public EngineIoMessage Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"“{nameof(value)}”不能为 null 或空。", nameof(value));
            }

            var message = value.Length > 1
                ? new EngineIoMessage((EngineIoMessageType)value[0] - '0', value.Substring(1))
                : new EngineIoMessage((EngineIoMessageType)value[0] - '0');
            return message;
        }

        public EngineIoMessage Decode(byte[] rawData)
        {
            if (rawData is null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }

            var message = new EngineIoMessage((EngineIoMessageType)rawData[0], rawData.Skip(1).ToArray());

            return message;
        }

        #endregion Decode
    }
}