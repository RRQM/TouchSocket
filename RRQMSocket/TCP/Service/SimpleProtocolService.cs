//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 简单协议服务器
    /// </summary>
    public class SimpleProtocolService : ProtocolService<SimpleProtocolSocketClient>
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<SimpleProtocolSocketClient> Received;

        /// <summary>
        /// 成功连接后创建（或从对象池中获得）辅助类,
        /// 用户可以在该方法中再进行自定义设置
        /// </summary>
        public event RRQMCreateSocketClientEventHandler<SimpleProtocolSocketClient> CreateSocketClient;

        /// <summary>
        /// 预处理流
        /// </summary>
        public event RRQMStreamOperationEventHandler<SimpleProtocolSocketClient> BeforeReceiveStream;

        /// <summary>
        /// 收到流数据
        /// </summary>
        public event RRQMStreamStatusEventHandler<SimpleProtocolSocketClient> ReceivedStream;


        /// <summary>
        /// 接收辅助类
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="createOption"></param>
        protected override void OnCreateSocketClient(SimpleProtocolSocketClient socketClient, CreateOption createOption)
        {
            this.CreateSocketClient?.Invoke(socketClient, createOption);
            socketClient.Received += this.OnReceive;
            socketClient.BeforeReceiveStream += this.OnBeforeReceiveStream;
            socketClient.ReceivedStream += this.OnReceivedStream;
        }

        private void OnReceive(SimpleProtocolSocketClient socketClient, short? procotol, ByteBlock byteBlock)
        {
            this.Received?.Invoke(socketClient, procotol, byteBlock);
        }

        private void OnBeforeReceiveStream(SimpleProtocolSocketClient client, StreamOperationEventArgs e)
        {
            this.BeforeReceiveStream?.Invoke(client, e);
        }

        private void OnReceivedStream(SimpleProtocolSocketClient client, StreamStatusEventArgs e)
        {
            this.ReceivedStream?.Invoke(client, e);
        }
    }
}