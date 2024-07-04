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

namespace TouchSocket.SocketIo
{
    internal sealed class SocketIoCore : ISocketIoCore
    {
        #region 字段

        private readonly WaitHandlePool<ISocketIoMessage> m_waitHandlePoolForEmit = new WaitHandlePool<ISocketIoMessage>();
        private IEngineIo m_engineIO;

        #endregion 字段

        public SocketIoCore()
        {
        }

        public EngineIoVersion EIO { get; set; }

        public IEngineIo EngineIo
        {
            get
            {
                if (this.m_engineIO != null)
                {
                    return this.m_engineIO;
                }

                this.m_engineIO = this.EIO switch
                {
                    EngineIoVersion.V3 => new EngineIo3(this.TransportType),
                    _ => new EngineIo4(this.TransportType),
                };
                return this.m_engineIO;
            }
        }

        public string Namespace { get; set; }
       
        public ISocketIoSerializer Serializer { get; set; } = new NewtonsoftJsonSerializer();

        public string Sid { get; set; }

        public EngineIoTransportType TransportType { get; set; }
        public Func<List<DataItem>,Task> SendAsyncAction { get; set; }

        public EngineIoMessage Decode(string value)
        {
            return this.EngineIo.Decode(value);
        }

        #region Serializer

        public ISocketIoMessage CreateMessage(SocketIoMessageType messageType)
        {
            return this.Serializer.CreateMessage(this.EIO, messageType);
        }

        public ISocketIoMessage Decode(in EngineIoMessage message)
        {
            return this.Serializer.Decode(this.EIO, message);
        }

        public object Deserialize(Type targetType, in ISocketIoMessage message, int index)
        {
            return this.Serializer.Deserialize(this.EIO, targetType, message, index);
        }

        public IHandshakeMessage DeserializeHandshakeMessage(in EngineIoMessage message)
        {
            return this.Serializer.DeserializeHandshakeMessage(this.EIO, message);
        }

        public List<DataItem> SerializeAck(int? packetId, string nsp, object[] data)
        {
            return this.Serializer.SerializeAck(this.EIO, packetId, nsp, data);
        }

        public List<DataItem> SerializeEvent(string eventName, int? packetId, string nsp, object[] data)
        {
            return this.Serializer.SerializeEvent(this.EIO, eventName, packetId, nsp, data);
        }

        //public List<DataItem> SerializeClose(string msg)
        //{
        //    var  socketIoMessage= this.Serializer.CreateMessage(this.EIO,  SocketIoMessageType.Disconnected);

        //    this.EngineIo.EncodeToString(socketIoMessage);
        //}

        #endregion Serializer

        #region Emit

        public async Task AckAsync(int packetId, object[] data)
        {
            var dataItems = this.SerializeAck(packetId, this.Namespace, data);
            await this.SendAsync(dataItems);
        }

        public async Task EmitAsync(string eventName, object[] data)
        {
            var dataItems = this.SerializeEvent(eventName, null, this.Namespace, data);
            await this.SendAsync(dataItems);
        }

        public async Task<ISocketIoResponse> EmitWithAckAsync(string eventName, object[] data, int millisecondsTimeout, CancellationToken token)
        {
            var waitData = this.m_waitHandlePoolForEmit.GetWaitDataAsync(out var sign);
            var dataItems = this.SerializeEvent(eventName, sign, this.Namespace, data);
            await this.SendAsync(dataItems);
            waitData.SetCancellationToken(token);
            (await waitData.WaitAsync(millisecondsTimeout)).ThrowIfNotRunning();
            return new InternalSocketIoResponse(waitData.WaitResult, this);
        }

        #endregion Emit

        #region Send

        protected Task SendAsync(List<DataItem> dataItems)
        {
            return this.SendAsyncAction.Invoke(dataItems);
        }

        #endregion Send

        #region ReceivedAck

        public Task ReceivedAck(ISocketIoMessage socketIOMessage)
        {
            this.m_waitHandlePoolForEmit.SetRun(socketIOMessage);
            return EasyTask.CompletedTask;
        }

        public Task ReceivedBinaryAck(ISocketIoMessage socketIOMessage)
        {
            this.m_waitHandlePoolForEmit.SetRun(socketIOMessage);
            return EasyTask.CompletedTask;
        }

        #endregion ReceivedAck
    }
}