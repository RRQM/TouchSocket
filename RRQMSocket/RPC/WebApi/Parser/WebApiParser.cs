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
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMSocket.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// WebApi解析器
    /// </summary>
    public sealed class WebApiParser : RPCParser, IService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiParser()
        {
            tcpService = new RRQMTcpService<RRQMSocketClient>();

            this.tcpService.CreatSocketCliect += this.OnCreatSocketCliect;
        }

        /// <summary>
        /// 在初次接收时
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="creatOption"></param>
        private void OnCreatSocketCliect(RRQMSocketClient socketClient, CreatOption creatOption)
        {
            if (creatOption.NewCreat)
            {
                socketClient.OnReceived = OnReceived;
                socketClient.DataHandlingAdapter = new Http.HttpDataHandlingAdapter(this.BufferLength);
            }
        }



        private RRQMTcpService<RRQMSocketClient> tcpService;

        /// <summary>
        /// 获取绑定状态
        /// </summary>
        public bool IsBind => this.tcpService.IsBind;

        /// <summary>
        /// 获取或设置缓存大小
        /// </summary>
        public int BufferLength { get { return this.tcpService.BufferLength; } set { this.tcpService.BufferLength = value; } }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool => this.tcpService.BytePool;

        /// <summary>
        /// 获取或设置日志记录器
        /// </summary>
        public ILog Logger { get { return this.tcpService.Logger; } set { this.tcpService.Logger = value; } }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Bind(int port, int threadCount = 1)
        {
            this.tcpService.Bind(port, threadCount);
        }


        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="iPHost">ip和端口号，格式如“127.0.0.1:7789”。IP可输入Ipv6</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Bind(IPHost iPHost, int threadCount)
        {
            this.tcpService.Bind(iPHost, threadCount);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="addressFamily">寻址方案</param>
        /// <param name="endPoint">绑定节点</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Bind(AddressFamily addressFamily, EndPoint endPoint, int threadCount)
        {
            this.tcpService.Bind(addressFamily, endPoint, threadCount);
        }

        private void OnReceived(ByteBlock byteBlock, object obj)
        {
            HttpRequest httpRequest = (HttpRequest)obj;
           
        }

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected override void EndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="methodInstances"></param>
        protected override void InitializeServers(MethodInstance[] methodInstances)
        {
            
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            this.tcpService.Dispose();
        }
    }
}