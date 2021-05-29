//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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
using System;
using System.Net.Sockets;
using System.Text;

namespace RRQMSocket
{
    /// <summary>
    /// RRQM协议助手
    /// </summary>
    public class RRQMAgreementHelper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        public RRQMAgreementHelper(IClient client)
        {
            this.mainSocket = client.MainSocket;
            this.bytePool = client.BytePool;
        }

        private Socket mainSocket;
        private BytePool bytePool;

        #region 方法

        /// <summary>
        /// 发送简单协议信息
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="text"></param>
        public void SocketSend(int agreement, string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text == null ? string.Empty : text);
            this.SocketSend(agreement, data);
        }

        /// <summary>
        /// 发送简单协议状态
        /// </summary>
        /// <param name="agreement"></param>
        public void SocketSend(int agreement)
        {
            this.SocketSend(agreement, string.Empty);
        }

        /// <summary>
        /// 发送int数字
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="number"></param>
        public void SocketSend(int agreement, int number)
        {
            byte[] data = BitConverter.GetBytes(number);
            this.SocketSend(agreement, data);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="dataBuffer"></param>
        public void SocketSend(int agreement, byte[] dataBuffer)
        {
            this.SocketSend(agreement, dataBuffer, 0, dataBuffer.Length);
        }

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="dataByteBlock"></param>
        public void SocketSend(int agreement, ByteBlock dataByteBlock)
        {
            this.SocketSend(agreement, dataByteBlock.Buffer, 0, (int)dataByteBlock.Length);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="status"></param>
        /// <param name="dataBuffer"></param>
        public void SocketSend(int agreement, byte status, byte[] dataBuffer)
        {
            ByteBlock byteBlock = this.bytePool.GetByteBlock(dataBuffer.Length + 1);

            byteBlock.Write(status);
            byteBlock.Write(dataBuffer);
            try
            {
                this.SocketSend(agreement, byteBlock.Buffer, 0, (int)byteBlock.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 发送流
        /// </summary>
        /// <param name="dataByteBlock"></param>
        public void SocketSend(ByteBlock dataByteBlock)
        {
            int dataLen = (int)dataByteBlock.Position + 4;
            ByteBlock byteBlock = this.bytePool.GetByteBlock(dataLen);
            byte[] lenBytes = BitConverter.GetBytes(dataLen);

            byteBlock.Write(lenBytes);

            byteBlock.Write(dataByteBlock.Buffer, 0, (int)dataByteBlock.Position);
            try
            {
                this.mainSocket.Send(byteBlock.Buffer, 0, (int)byteBlock.Position, SocketFlags.None);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SocketSend(int agreement, byte[] dataBuffer, int offset, int length)
        {
            int dataLen = length - offset + 8;
            ByteBlock byteBlock = this.bytePool.GetByteBlock(dataLen);
            byte[] lenBytes = BitConverter.GetBytes(dataLen);
            byte[] agreementBytes = BitConverter.GetBytes(agreement);

            byteBlock.Write(lenBytes);
            byteBlock.Write(agreementBytes);

            byteBlock.Write(dataBuffer, offset, length);
            try
            {
                this.mainSocket.Send(byteBlock.Buffer, 0, (int)byteBlock.Position, SocketFlags.None);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        #endregion 方法
    }
}