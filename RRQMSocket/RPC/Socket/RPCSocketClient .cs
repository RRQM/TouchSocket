//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC服务器辅助类
    /// </summary>
    public sealed class RPCSocketClient : TcpSocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RPCSocketClient()
        {
            this.waitHandles = new RRQMWaitHandle<WaitBytes>();
        }

        internal event RRQMByteBlockEventHandler OnReceivedRequest;

        private RRQMWaitHandle<WaitBytes> waitHandles;
        internal RRQMAgreementHelper agreementHelper;

        /// <summary>
        /// 等待字节返回
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="r"></param>
        internal void Agreement_110(byte[] buffer, int r)
        {
            WaitBytes waitBytes = SerializeConvert.BinaryDeserialize<WaitBytes>(buffer, 4, r - 4);
            this.waitHandles.SetRun(waitBytes.Sign, waitBytes);
        }

        /// <summary>
        /// 发送字节并返回
        /// </summary>
        /// <param name="data"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public byte[] SendBytesWaitReturn(byte[] data, int waitTime = 3)
        {
            WaitData<WaitBytes> waitData = this.waitHandles.GetWaitData();
            waitData.WaitResult.Bytes = data;
            try
            {
                byte[] buffer = SerializeConvert.BinarySerialize(waitData.WaitResult);
                agreementHelper.SocketSend(110, buffer);
            }
            catch (Exception e)
            {
                throw new RRQMRPCException(e.Message);
            }

            waitData.Wait(waitTime * 1000);

            waitData.Wait(waitTime * 1000);
            WaitBytes waitBytes = waitData.WaitResult;
            waitData.Dispose();

            if (waitBytes.Status == 0)
            {
                throw new RRQMTimeoutException("等待结果超时");
            }
            else if (waitBytes.Status == 2)
            {
                throw new RRQMRPCException(waitBytes.Message);
            }

            return waitBytes.Bytes;
        }

        /// <summary>
        /// 向RPC发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Send(byte[] buffer, int offset, int length)
        {
            agreementHelper.SocketSend(111, buffer, offset, length);
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock,object obj)
        {
            OnReceivedRequest?.Invoke(this, byteBlock);
        }
    }
}