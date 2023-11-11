using System;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace RealityProxyDmtpRpcConsoleApp
{
    /// <summary>
    /// 调用前先启动DmtpRpcServerConsoleApp项目
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var myDmtpRpcRealityProxy = new MyDmtpRpcRealityProxy<IMyRpcServer>();
            
            var myRpcServer = myDmtpRpcRealityProxy.GetTransparentProxy();

            var result = myRpcServer.Add(10, 20);
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 新建一个类，按照需要，继承DmtpRpcRealityProxy，亦或者RpcRealityProxy基类。
    /// 然后实现抽象方法，主要是能获取到调用的IRpcClient派生接口。
    /// </summary>
    class MyDmtpRpcRealityProxy<T> : DmtpRpcRealityProxy<T>
    {
        private readonly TcpDmtpClient m_client;

        public MyDmtpRpcRealityProxy()
        {
            this.m_client = GetTcpDmtpClient();
        }

        private static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetRemoteIPHost("127.0.0.1:7789")
            .SetDmtpOption(new DmtpOption()
            {
                VerifyToken = "Dmtp"
            }));
            client.Connect();
            client.Logger.Info($"连接成功，Id={client.Id}");
            return client;
        }

        public override IDmtpRpcActor GetClient()
        {
            return m_client.GetDmtpRpcActor();
        }
    }

    interface IMyRpcServer
    {
        /// <summary>
        /// 将两个数相加
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [DmtpRpc(true)]//使用函数名直接调用
        int Add(int a, int b);
    }
}