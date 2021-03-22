using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// UDP协议客户端
    /// </summary>
    public class UdpRPCClient : IUdpRPCClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpRPCClient():this(new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20))
        { 
        
        }
      
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool"></param>
        public UdpRPCClient(BytePool bytePool)
        {
            this.BytePool = bytePool;
            this.udpSession = new RRQMUdpSession();
            this.udpSession.OnReceivedData += this.UdpSession_OnReceivedData;
        }

        private void UdpSession_OnReceivedData(EndPoint remoteEndpoint, ByteBlock e)
        {
           
        }

        private RRQMUdpSession udpSession;
        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get; private set; }

        public RPCProxyInfo GetProxyInfo(string proxyToken)
        {
           
        }

        public void InitializedRPC()
        {
           
        }

        public void RPCInvoke(string method, ref object[] parameters, int waitTime = 3)
        {
           
        }

    }
}
