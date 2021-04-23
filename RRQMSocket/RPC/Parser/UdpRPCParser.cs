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
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using RRQMCore.ByteManager;
using RRQMCore.Log;
using RRQMCore.Serialization;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// UDP RPC解释器
    /// </summary>
    public class UdpRPCParser : UdpSession, IRPCParser
    {
        /// <summary>
        /// 调用方法
        /// </summary>
        public event Action<IRPCParser, RPCContext> InvokeMethod;

        /// <summary>
        /// 获取代理文件
        /// </summary>
        public Func<string, IRPCParser, RPCProxyInfo> GetProxyInfo { get; set; }

        /// <summary>
        /// 初始化服务
        /// </summary>
        public Func<IRPCParser, List<MethodItem>> InitMethodServer { get; set; }

        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 调用结束
        /// </summary>
        /// <param name="context"></param>
        public void EndInvokeMethod(RPCContext context)
        {
        }

        /// <summary>
        /// 接收处理数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
            int agreement = BitConverter.ToInt32(buffer, 0);

            switch (agreement)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        try
                        {
                            string proxyToken = null;
                            if (r - 4 > 0)
                            {
                                proxyToken = Encoding.UTF8.GetString(buffer, 4, r - 4);
                            }
                            this.UDPSend(100, remoteEndPoint, SerializeConvert.RRQMBinarySerialize(this.GetProxyInfo?.Invoke(proxyToken, this), true));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"UDP错误代码: 100, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 101:/*函数式调用*/
                    {
                        try
                        {
                            RPCContext content = RPCContext.Deserialize(buffer, 4);
                            content.Flag = remoteEndPoint;
                            this.InvokeMethod?.Invoke(this, content);
                            if (content.Feedback != 0)
                            {
                                this.UDPSend(101, remoteEndPoint, new byte[0]);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"UDP错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*连接初始化*/
                    {
                        try
                        {
                            UDPSend(102, remoteEndPoint, SerializeConvert.RRQMBinarySerialize(this.InitMethodServer?.Invoke(this), true));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
            }
        }

        private void UDPSend(int agreement, EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length + 4);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(agreement));
                byteBlock.Write(buffer, offset, length);
                this.SendTo(byteBlock.Buffer, 0, (int)byteBlock.Length, endPoint);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void UDPSend(int agreement, EndPoint endPoint, byte[] buffer)
        {
            this.UDPSend(agreement, endPoint, buffer, 0, buffer.Length);
        }
    }
}