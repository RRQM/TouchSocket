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
//using System;
//using Microsoft.CSharp;
//using System.CodeDom.Compiler;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using RRQMCore.ByteManager;
//using System.Net;

//#if !NET45_OR_GREATER
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.Extensions.DependencyModel;
//using Microsoft.CodeAnalysis.Text;
//using Microsoft.CodeAnalysis.Emit;
//#endif

//namespace RRQMSocket.RPC
//{
//    /// <summary>
//    /// 通讯服务端主类
//    /// </summary>
//    public sealed class TcpRPCService : RPCService, IRPCService
//    {
//        /// <summary>
//        /// 构造函数
//        /// </summary>
//        public TcpRPCService()
//        {
//            tcpService = new RRQMTokenTcpService<RPCSocketClient>();
//            tcpService.CreatSocketCliect += this.TcpService_CreatSocketCliect;
//        }

//        private void TcpService_CreatSocketCliect(RPCSocketClient tcpSocketClient, bool newCreat)
//        {
//            if (newCreat)
//            {
//                tcpSocketClient.serverMethodStore = this.serverMethodStore;
//                tcpSocketClient.clientMethodStore = this.clientMethodStore;
//                tcpSocketClient.SerializeConverter = this.SerializeConverter;
//                tcpSocketClient.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
//            }
//            tcpSocketClient.agreementHelper = new RRQMAgreementHelper(tcpSocketClient.MainSocket, this.BytePool);
//        }

//        private RRQMTokenTcpService<RPCSocketClient> tcpService;

//        /// <summary>
//        /// 绑定状态
//        /// </summary>
//        public override bool IsBind => this.tcpService.IsBind;

//        /// <summary>
//        /// 获取内存池实例
//        /// </summary>
//        public override BytePool BytePool => this.tcpService.BytePool;
        
//        /// <summary>
//        /// 通过ID获得通信实例
//        /// </summary>
//        /// <param name="iDToken"></param>
//        /// <returns></returns>
//        public override ISocketClient GetSocketClient(string iDToken)
//        {
//            return this.tcpService.SocketClients[iDToken];
//        }

//        /// <summary>
//        /// 绑定IP及端口号
//        /// </summary>
//        /// <param name="setting"></param>
//        public override void Bind(BindSetting setting)
//        {
//            this.tcpService.Bind(setting);
//        }

//        /// <summary>
//        /// 绑定IP及端口号
//        /// </summary>
//        /// <param name="endPoint"></param>
//        /// <param name="threadCount"></param>
//        public override void Bind(EndPoint endPoint, int threadCount)
//        {
//            this.tcpService.Bind(endPoint, threadCount);
//        }

//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        public override void Dispose()
//        {
//            this.tcpService.Dispose();
//        }
//    }

//}

