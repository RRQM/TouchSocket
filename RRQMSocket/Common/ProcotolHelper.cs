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
    public class ProcotolHelper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        public ProcotolHelper(IClient client)
        {
            this.mainSocket = client.MainSocket;
            this.bytePool = client.BytePool;
        }

        private Socket mainSocket;
        private BytePool bytePool;

        #region 方法

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
        public void SocketSend(short procotol, byte[] dataBuffer, int offset, int length, bool reserved=false)
        {
            int dataLen = length - offset + 6;
            if (reserved)
            {
                byte[] lenBytes1 = BitConverter.GetBytes(dataLen);
                byte[] agreementBytes1 = BitConverter.GetBytes(procotol);
                Array.Copy(lenBytes1, 0, dataBuffer, offset, 4);
                Array.Copy(agreementBytes1, 0, dataBuffer, 4 + offset, 2);
                this.mainSocket.Send(dataBuffer, 0, length, SocketFlags.None);
                return;
            }
            ByteBlock byteBlock = this.bytePool.GetByteBlock(dataLen);
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
                this.mainSocket.Send(byteBlock.Buffer, 0, (int)byteBlock.Position, SocketFlags.None);
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

        #endregion 方法
    }
}