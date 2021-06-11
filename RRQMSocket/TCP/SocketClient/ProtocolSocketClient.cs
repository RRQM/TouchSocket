using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// 协议辅助类
    /// </summary>
    public abstract class ProtocolSocketClient : SocketClient
    {
        internal RRQMAgreementHelper agreementHelper;

        /// <summary>
        /// 接收之前
        /// </summary>
        protected override void OnBeforeReceive()
        {
            base.OnBeforeReceive();
            this.agreementHelper = new RRQMAgreementHelper(this);
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public sealed override void Send(byte[] buffer, int offset, int length)
        {
            this.agreementHelper.SocketSend(-1, buffer, offset, length);
        }

        /// <summary>
        /// 发送字节流(仍然为同步发送)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void SendAsync(byte[] buffer, int offset, int length)
        {
            this.agreementHelper.SocketSend(-1, buffer, offset, length);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(short agreement, byte[] buffer, int offset, int length)
        {
            if (agreement > 0)
            {
                this.agreementHelper.SocketSend(agreement, buffer, offset, length);
            }
            else
            {
                throw new RRQMException("小等于0的协议为系统使用协议");
            }
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="dataBuffer"></param>
        public void SocketSend(short agreement, byte[] dataBuffer)
        {
            this.Send(agreement, dataBuffer, 0, dataBuffer.Length);
        }

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="dataByteBlock"></param>
        public void Send(short agreement, ByteBlock dataByteBlock)
        {
            this.Send(agreement, dataByteBlock.Buffer, 0, (int)dataByteBlock.Length);
        }

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="agreement"></param>
        public void Send(short agreement)
        {
            this.Send(agreement, new byte[0], 0, 0);
        }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="id"></param>
        protected override void ResetID(string id)
        {
            this.Service.ResetID(this.id,id);
        }

        /// <summary>
        /// 密封方法
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            short agreement = BitConverter.ToInt16(byteBlock.Buffer, 0);
            switch (agreement)
            {
                case 0:
                    {
                        try
                        {
                            string id = Encoding.UTF8.GetString(byteBlock.Buffer, 2, (int)byteBlock.Length - 2);
                            base.ResetID(id);
                            this.agreementHelper.SocketSend(0, Encoding.UTF8.GetBytes(this.id));
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "重置ID错误", ex);
                        }

                        break;
                    }
                case -1:
                    {
                        ByteBlock newByteBlock = this.BytePool.GetByteBlock(byteBlock.Length - 2);
                        try
                        {
                            newByteBlock.Write(byteBlock.Buffer, 2, (int)byteBlock.Length - 2);
                            this.HandleNormalData(newByteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "处理无协议数据异常", ex);
                        }
                        finally
                        {
                            newByteBlock.Dispose();
                        }
                        break;
                    }
                default:
                    {
                        HandleProtocolData(agreement, byteBlock);
                        break;
                    }

            }
        }

        /// <summary>
        /// 收到协议数据，由于性能考虑，
        /// byteBlock数据源并未剔除协议数据，
        /// 所以真实数据起点为2，
        /// 长度为Length-2。
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleProtocolData(short agreement, ByteBlock byteBlock);

        /// <summary>
        /// 处理无协议数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected abstract void HandleNormalData(ByteBlock byteBlock);
    }
}
