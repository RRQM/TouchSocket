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
using System;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// RRQM协议助手
    /// </summary>
    public class ProcotolHelper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        public ProcotolHelper(ISocketClient client)
        {
            this.client = client;
            this.mainSocket = client.MainSocket;
            this.bytePool = client.BytePool;
            this.separateThread = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        /// <param name="onError"></param>
        public ProcotolHelper(IUserTcpClient client, Action<Exception> onError)
        {
            this.client = client;
            this.mainSocket = client.MainSocket;
            this.bytePool = client.BytePool;
            this.separateThread = client.SeparateThreadSend;
            this.asyncSender = new AsyncSender(client.MainSocket, client.MainSocket.RemoteEndPoint, onError);
        }

        private Socket mainSocket;
        private BytePool bytePool;
        private AsyncSender asyncSender;
        private bool separateThread;

        private ITcpClient client;
        /// <summary>
        /// 客户端
        /// </summary>
        public ITcpClient Client
        {
            get { return client; }
        }


        #region 同步方法

        /// <summary>
        /// 发送简单协议
        /// </summary>
        /// <param name="procotol"></param>
        public void SocketSend(short procotol)
        {
            this.SocketSend(procotol, new byte[0], 0, 0);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataBuffer"></param>
        public void SocketSend(short procotol, byte[] dataBuffer)
        {
            this.SocketSend(procotol, dataBuffer, 0, dataBuffer.Length);
        }

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataByteBlock"></param>
        public void SocketSend(short procotol, ByteBlock dataByteBlock)
        {
            this.SocketSend(procotol, dataByteBlock.Buffer, 0, (int)dataByteBlock.Length);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="reserved"></param>
        public void SocketSend(short procotol, byte[] dataBuffer, int offset, int length, bool reserved = false)
        {
            int dataLen;
            if (reserved)
            {
                dataLen = length - 4;
                byte[] lenBytes1 = BitConverter.GetBytes(dataLen);
                byte[] agreementBytes1 = BitConverter.GetBytes(procotol);
                Array.Copy(lenBytes1, 0, dataBuffer, offset, 4);
                Array.Copy(agreementBytes1, 0, dataBuffer, 4 + offset, 2);
                this.mainSocket.Send(dataBuffer, 0, length, SocketFlags.None);
                return;
            }
            dataLen = length + 2;
            ByteBlock byteBlock = this.bytePool.GetByteBlock(dataLen + 4);
            byte[] lenBytes = BitConverter.GetBytes(dataLen);
            byte[] agreementBytes = BitConverter.GetBytes(procotol);

            byteBlock.Write(lenBytes);
            byteBlock.Write(agreementBytes);
            if (length > 0)
            {
                byteBlock.Write(dataBuffer, offset, length);
            }
            try
            {
                this.mainSocket.Send(byteBlock.Buffer, 0, byteBlock.Len, SocketFlags.None);
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

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 发送简单协议
        /// </summary>
        /// <param name="procotol"></param>
        public void SocketSendAsync(short procotol)
        {
            this.SocketSendAsync(procotol, new byte[0], 0, 0);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataBuffer"></param>
        public void SocketSendAsync(short procotol, byte[] dataBuffer)
        {
            this.SocketSendAsync(procotol, dataBuffer, 0, dataBuffer.Length);
        }

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataByteBlock"></param>
        public void SocketSendAsync(short procotol, ByteBlock dataByteBlock)
        {
            this.SocketSendAsync(procotol, dataByteBlock.Buffer, 0, dataByteBlock.Len);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SocketSendAsync(short procotol, byte[] dataBuffer, int offset, int length)
        {
            int dataLen;
            dataLen = length - offset + 2;
            ByteBlock byteBlock = this.bytePool.GetByteBlock(dataLen + 4);
            byte[] lenBytes = BitConverter.GetBytes(dataLen);
            byte[] agreementBytes = BitConverter.GetBytes(procotol);

            byteBlock.Write(lenBytes);
            byteBlock.Write(agreementBytes);
            if (length > 0)
            {
                byteBlock.Write(dataBuffer, offset, length);
            }
            try
            {
                byte[] data = byteBlock.ToArray();
                if (this.separateThread)
                {
                    this.asyncSender.AsyncSend(data, 0, data.Length);
                }
                else
                {
                    this.mainSocket.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);
                }
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

        #endregion 异步方法
    }
}