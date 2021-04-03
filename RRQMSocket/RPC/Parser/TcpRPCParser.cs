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
using RRQMCore.Log;
using RRQMCore.Serialization;
using System;
using System.Text;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// TCP RPC解释器
    /// </summary>
    public class TcpRPCParser : TokenTcpService<RPCSocketClient>, IRPCParser
    {
        /// <summary>
        /// 调用方法
        /// </summary>
        public event Action<IRPCParser, RPCContext> InvokeMethod;

        /// <summary>
        /// 服务器函数映射
        /// </summary>
        private MethodStore serverMethodStore;

        /// <summary>
        /// 客户端函数映射
        /// </summary>
        private MethodStore clientMethodStore;

        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 初始化函数映射
        /// </summary>
        /// <param name="serverMethodStore"></param>
        /// <param name="clientMethodStore"></param>
        public void InitMethodStore(MethodStore serverMethodStore, MethodStore clientMethodStore)
        {
            this.serverMethodStore = serverMethodStore;
            this.clientMethodStore = clientMethodStore;
        }

        /// <summary>
        /// 初创
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="creatOption"></param>
        protected override void OnCreatSocketCliect(RPCSocketClient tcpSocketClient, CreatOption creatOption)
        {
            if (creatOption.NewCreat)
            {
                tcpSocketClient.Logger = this.Logger;
                tcpSocketClient.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
                tcpSocketClient.OnReceivedRequest += this.TcpSocketClient_OnReceivedRequest;
            }
            tcpSocketClient.agreementHelper = new RRQMAgreementHelper(tcpSocketClient.MainSocket, tcpSocketClient.BytePool);
        }

        private void TcpSocketClient_OnReceivedRequest(object sender, ByteBlock byteBlock)
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
                            RPCSocketClient socketClient = (RPCSocketClient)sender;
                            string proxyToken = null;
                            if (r - 4 > 0)
                            {
                                proxyToken = Encoding.UTF8.GetString(buffer, 4, r - 4);
                            }
                            socketClient.agreementHelper.SocketSend(100, SerializeConvert.BinarySerialize(this.serverMethodStore.GetProxyInfo(proxyToken)));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 100, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 101:/*函数式调用*/
                    {
                        try
                        {
                            RPCContext content = RPCContext.Deserialize(buffer, 4);
                            content.Flag = sender;
                            InvokeMethod?.Invoke(this, content);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*连接初始化*/
                    {
                        try
                        {
                            ((RPCSocketClient)sender).agreementHelper.SocketSend(102, SerializeConvert.BinarySerialize(this.clientMethodStore.GetAllMethodItem()));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 110:/*函数式调用返回*/
                    {
                        try
                        {
                            ((RPCSocketClient)sender).Agreement_110(buffer, r);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 110, 错误详情:{e.Message}");
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 调用结束
        /// </summary>
        /// <param name="context"></param>
        public void EndInvokeMethod(RPCContext context)
        {
            if (context.Feedback == 0)
            {
                return;
            }
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            try
            {
                context.Serialize(byteBlock);
                ((RPCSocketClient)context.Flag).agreementHelper.SocketSend(101, byteBlock);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, ex.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}