//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//using RRQMCore.ByteManager;
//using System.Net;

//namespace RRQMSocket.RPC
//{
//    /// <summary>
//    /// 通讯服务端主类
//    /// </summary>
//    public sealed class UdpRPCService : RPCService
//    {
//        /// <summary>
//        /// 构造函数
//        /// </summary>
//        public UdpRPCService()
//        {
//            udpSession = new RRQMUdpSession();
//            udpSession.OnReceivedData += this.UdpSession_OnReceivedData;
//        }

      

//        private RRQMUdpSession udpSession;

//        /// <summary>
//        /// 绑定状态
//        /// </summary>
//        public override bool IsBind => this.udpSession.IsBind;

//        /// <summary>
//        /// 获取内存池实例
//        /// </summary>
//        public override BytePool BytePool => this.udpSession.BytePool;

//        /// <summary>
//        /// 绑定监听
//        /// </summary>
//        /// <param name="setting"></param>
//        public override void Bind(BindSetting setting)
//        {
//            udpSession.Bind(setting);
//        }

//        /// <summary>
//        /// 绑定监听
//        /// </summary>
//        /// <param name="endPoint"></param>
//        /// <param name="threadCount"></param>
//        public override void Bind(EndPoint endPoint, int threadCount)
//        {
//            udpSession.Bind(endPoint, threadCount);
//        }

//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        public override void Dispose()
//        {
//            this.udpSession.Dispose();
//        }


//        /// <summary>
//        /// 实现方法，UDP中无意义
//        /// </summary>
//        /// <param name="iDToken"></param>
//        /// <returns></returns>
//        public override ISocketClient GetSocketClient(string iDToken)
//        {
//            return null;
//        }

       
//    }
//}