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
        /// <summary>
        /// 辅助发送器
        /// </summary>
        internal ProcotolHelper procotolHelper;
        private static readonly Dictionary<short, string> usedProtocol = new Dictionary<short, string>();
        /// <summary>
        /// 接收之前
        /// </summary>
        protected override void OnBeforeReceive()
        {
            base.OnBeforeReceive();
            this.procotolHelper = new ProcotolHelper(this);
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
        }
        /// <summary>
        /// 添加已被使用的协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="describe"></param>
        protected static void AddUsedProtocol(short procotol, string describe)
        {
            usedProtocol.Add(procotol, describe);
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
            this.procotolHelper.SocketSend(-1, buffer, offset, length);
        }

        /// <summary>
        /// 发送字节流(仍然为同步发送)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void SendAsync(byte[] buffer, int offset, int length)
        {
            this.procotolHelper.SocketSend(-1, buffer, offset, length);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(short procotol, byte[] buffer, int offset, int length)
        {
            if (!usedProtocol.ContainsKey(procotol))
            {
                this.InternalSend(procotol, buffer, offset, length);
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in usedProtocol.Keys)
                {
                    stringBuilder.AppendLine($"协议{item}已被使用，描述为：{usedProtocol[item]}");
                }
                throw new RRQMException(stringBuilder.ToString());
            }
        }

        /// <summary>
        /// 内部发送，不会进行协议检测
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="reserved"></param>
        protected void InternalSend(short procotol, byte[] buffer, int offset, int length, bool reserved = false)
        {
            if (procotol > 0)
            {
                this.procotolHelper.SocketSend(procotol, buffer, offset, length,reserved);
            }
            else
            {
                throw new RRQMException("小等于0的协议为系统使用协议");
            }
        }

        /// <summary>
        /// 内部发送，不会进行协议检测
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        /// <param name="reserved"></param>
        protected void InternalSend(short procotol, ByteBlock byteBlock, bool reserved = false)
        {
            this.InternalSend(procotol,byteBlock.Buffer,0,(int)byteBlock.Length,reserved);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataBuffer"></param>
        public void SocketSend(short procotol, byte[] dataBuffer)
        {
            this.Send(procotol, dataBuffer, 0, dataBuffer.Length);
        }

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataByteBlock"></param>
        public void Send(short procotol, ByteBlock dataByteBlock)
        {
            this.Send(procotol, dataByteBlock.Buffer, 0, (int)dataByteBlock.Length);
        }

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="procotol"></param>
        public void Send(short procotol)
        {
            this.Send(procotol, new byte[0], 0, 0);
        }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="id"></param>
        protected override void ResetID(string id)
        {
            this.Service.ResetID(this.id, id);
        }

        /// <summary>
        /// 密封方法
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            short procotol = BitConverter.ToInt16(byteBlock.Buffer, 0);
            switch (procotol)
            {
                case 0:
                    {
                        try
                        {
                            string id = Encoding.UTF8.GetString(byteBlock.Buffer, 2, (int)byteBlock.Length - 2);
                            base.ResetID(id);
                            this.procotolHelper.SocketSend(0, Encoding.UTF8.GetBytes(this.id));
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "重置ID错误", ex);
                        }

                        break;
                    }
                case -1:
                    {
                        try
                        {
                            byte[] data = new byte[(int)byteBlock.Length - 2];
                            byteBlock.Position = 2;
                            byteBlock.Read(data);
                            HandleProtocolData(null, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "处理无协议数据异常", ex);
                        }
                        break;
                    }
                default:
                    {
                        HandleProtocolData(procotol, byteBlock);
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
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleProtocolData(short? procotol, ByteBlock byteBlock);
    }
}
