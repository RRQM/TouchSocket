////------------------------------------------------------------------------------
////  此代码版权归作者本人若汝棋茗所有
////  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
////  CSDN博客：https://blog.csdn.net/qq_40374647
////  哔哩哔哩视频：https://space.bilibili.com/94253567
////  Gitee源代码仓库：https://gitee.com/RRQM_Home
////  Github源代码仓库：https://github.com/RRQM
////  交流QQ群：234762506
////  感谢您的下载和使用
////------------------------------------------------------------------------------
////------------------------------------------------------------------------------
//using RRQMCore.ByteManager;
//using RRQMCore.Exceptions;
//using RRQMCore.Log;
//using RRQMCore.Serialization;
//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;

//namespace RRQMSocket.RPC.RRQMRPC
//{
//    /// <summary>
//    /// UDP RPC解释器
//    /// </summary>
//    public class UdpRPCParser : RRQMRPCParser,IService
//    {
//        /// <summary>
//        /// 构造函数
//        /// </summary>
//        public UdpRPCParser()
//        {
//            this.udpSession = new SimpleUdpSession();
//            this.udpSession.Received += this.UdpSession_OnReceivedData;
//        }

//        private void UdpSession_OnReceivedData(EndPoint remoteEndpoint, ByteBlock byteBlock)
//        {
//            byte[] buffer = byteBlock.Buffer;
//            int r = (int)byteBlock.Position;
//            int agreement = BitConverter.ToInt32(buffer, 0);

//            switch (agreement)
//            {
//                case 100:/*100，请求RPC文件*/
//                    {
//                        try
//                        {
//                            string proxyToken = null;
//                            if (r - 4 > 0)
//                            {
//                                proxyToken = Encoding.UTF8.GetString(buffer, 4, r - 4);
//                            }
//                            this.UDPSend(100, remoteEndpoint, SerializeConvert.RRQMBinarySerialize(this.GetProxyInfo(proxyToken, this), true));
//                        }
//                        catch (Exception e)
//                        {
//                            Logger.Debug(LogType.Error, this, $"UDP错误代码: 100, 错误详情:{e.Message}");
//                        }
//                        break;
//                    }

//                case 101:/*函数式调用*/
//                    {
//                        try
//                        {
//                            RpcContext content = RpcContext.Deserialize(buffer, 4);
//                            this.ExecuteContext(content, remoteEndpoint);
//                            if (content.Feedback != 0)
//                            {
//                                this.UDPSend(101, remoteEndpoint, new byte[0]);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            Logger.Debug(LogType.Error, this, $"UDP错误代码: 101, 错误详情:{e.Message}");
//                        }
//                        break;
//                    }
//                case 102:/*连接初始化*/
//                    {
//                        try
//                        {
//                            UDPSend(102, remoteEndpoint, SerializeConvert.RRQMBinarySerialize(this.GetRegisteredMethodItems(this), true));
//                        }
//                        catch (Exception e)
//                        {
//                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
//                        }
//                        break;
//                    }
//            }
//        }

//        private SimpleUdpSession udpSession;

//        /// <summary>
//        /// 获取通信实例
//        /// </summary>
//        public SimpleUdpSession Session => this.udpSession;

//        /// <summary>
//        /// 获取或设置日志记录仪
//        /// </summary>
//        public ILog Logger { get { return this.udpSession.Logger; } set { this.udpSession.Logger = value; } }

//        /// <summary>
//        /// 获取内存池实例
//        /// </summary>
//        public override BytePool BytePool => this.udpSession.BytePool;

//        /// <summary>
//        /// 服务器状态
//        /// </summary>
//        public ServerState ServerState => this.udpSession.ServerState;

//        /// <summary>
//        /// 服务器配置
//        /// </summary>
//        public ServerConfig ServerConfig => this.udpSession.ServerConfig;

//        /// <summary>
//        /// 调用结束
//        /// </summary>
//        /// <param name="methodInvoker"></param>
//        /// <param name="methodInstance"></param>
//        protected override void EndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
//        {
//            return;
//        }

//        private void UDPSend(int agreement, EndPoint endPoint, byte[] buffer, int offset, int length)
//        {
//            ByteBlock byteBlock = this.BytePool.GetByteBlock(length + 4);
//            try
//            {
//                byteBlock.Write(BitConverter.GetBytes(agreement));
//                byteBlock.Write(buffer, offset, length);
//                this.udpSession.SendTo(byteBlock.Buffer, 0, (int)byteBlock.Length, endPoint);
//            }
//            finally
//            {
//                byteBlock.Dispose();
//            }
//        }

//        private void UDPSend(int agreement, EndPoint endPoint, byte[] buffer)
//        {
//            this.UDPSend(agreement, endPoint, buffer, 0, buffer.Length);
//        }

        
//        /// <summary>
//        /// 设置
//        /// </summary>
//        /// <param name="serverConfig"></param>
//        public void Setup(ServerConfig serverConfig)
//        {
//            this.udpSession.Setup(serverConfig);
//        }

//        /// <summary>
//        /// 设置
//        /// </summary>
//        /// <param name="port"></param>
//        public void Setup(int port)
//        {
//            this.udpSession.Setup(port);
//        }

//        /// <summary>
//        /// 启动
//        /// </summary>
//        public void Start()
//        {
//            this.udpSession.Start();
//        }

//        /// <summary>
//        /// 停止
//        /// </summary>
//        public void Stop()
//        {
//            this.udpSession.Stop();
//        }

//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        public override void Dispose()
//        {
//            this.udpSession.Dispose();
//        }
//    }
//}