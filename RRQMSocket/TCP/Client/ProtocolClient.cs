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
using RRQMCore.Exceptions;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 协议客户端
    /// </summary>
    public abstract class ProtocolClient : TokenClient
    {
        private static readonly Dictionary<short, string> usedProtocol = new Dictionary<short, string>();

        private ProcotolHelper procotolHelper;

        private EventWaitHandle waitHandle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtocolClient()
        {
            waitHandle = new AutoResetEvent(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.waitHandle.Dispose();
        }

        /// <summary>
        /// 重新设置ID,并且同步到服务器
        /// </summary>
        /// <param name="id"></param>
        public override void ResetID(string id)
        {
            this.procotolHelper.SocketSend(0, Encoding.UTF8.GetBytes(id));
            if (this.waitHandle.WaitOne(5000))
            {
                return;
            }
            throw new RRQMException("同步ID超时");
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
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        public void Send(short procotol, byte[] buffer)
        {
            this.Send(procotol, buffer, 0, buffer.Length);
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
        /// 发送字节流(仍然为同步发送)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void SendAsync(byte[] buffer, int offset, int length)
        {
            this.procotolHelper.SocketSendAsync(-1, buffer, offset, length);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendAsync(short procotol, byte[] buffer, int offset, int length)
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
        /// 添加已被使用的协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="describe"></param>
        protected static void AddUsedProtocol(short procotol, string describe)
        {
            usedProtocol.Add(procotol, describe);
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
                            string id = Encoding.UTF8.GetString(byteBlock.Buffer, 2, byteBlock.Len - 2);
                            base.ResetID(id);
                            this.waitHandle.Set();
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
                        try
                        {
                            HandleProtocolData(procotol, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "处理协议数据异常", ex);
                        }
                        break;
                    }
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
                this.procotolHelper.SocketSend(procotol, buffer, offset, length, reserved);
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
            this.InternalSend(procotol, byteBlock.Buffer, 0, byteBlock.Len, reserved);
        }

        /// <summary>
        /// 内部发送，不会进行协议检测
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected void InternalSendAsync(short procotol, byte[] buffer, int offset, int length)
        {
            if (procotol > 0)
            {
                this.procotolHelper.SocketSendAsync(procotol, buffer, offset, length);
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
        protected void InternalSendAsync(short procotol, ByteBlock byteBlock)
        {
            this.InternalSendAsync(procotol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 载入配置，协议客户端数据处理适配器不可更改。
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            if (clientConfig.DataHandlingAdapter is FixedHeaderDataHandlingAdapter adapter)
            {
                adapter.FixedHeaderType = FixedHeaderType.Int;
                this.SetDataHandlingAdapter(adapter);
            }
            else
            {
                this.SetDataHandlingAdapter(new FixedHeaderDataHandlingAdapter());
            }
        }

        /// <summary>
        /// 连接到服务器时
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnectedService(MesEventArgs e)
        {
            this.procotolHelper = new ProcotolHelper(this, this.SeparateThreadSend);
            base.OnConnectedService(e);
        }
    }
}