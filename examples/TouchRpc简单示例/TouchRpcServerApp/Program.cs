using System;
using System.ComponentModel;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace TouchRpcServerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpTouchRpcService tcpTouchRpcService = CreateTcpTouchRpcService(7789);
            HttpTouchRpcService httpTouchRpcService = CreateHttpTouchRpcService(7790);
            UdpTouchRpc udpTouchRpc = CreateUdpTouchRpc(7791);

            string code = CodeGenerator.ConvertToCode("RpcProxy", CodeGenerator.Generator<MyRpcServer, TouchRpcAttribute>());
            File.WriteAllText("../../../RpcProxy.cs", code);
            Console.ReadKey();
        }

        private static TcpTouchRpcService CreateTcpTouchRpcService(int port)
        {
            var service = new TcpTouchRpcService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddFileLogger();
                   })
                   .ConfigureRpcStore(a =>
                   {
                       a.RegisterServer<MyRpcServer>();//注册服务
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
            return service;
        }

        private static HttpTouchRpcService CreateHttpTouchRpcService(int port)
        {
            var service = new HttpTouchRpcService();
            TouchSocketConfig config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddFileLogger();
                   })
                   .ConfigureRpcStore(a =>
                   {
                       a.RegisterServer<MyRpcServer>();//注册服务
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
            return service;
        }

        private static UdpTouchRpc CreateUdpTouchRpc(int port)
        {
            var service = new UdpTouchRpc();
            TouchSocketConfig config = new TouchSocketConfig()//配置
                   .SetBindIPHost(new IPHost(port))
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddFileLogger();
                   })
                   .ConfigureRpcStore(a =>
                   {
                       a.RegisterServer<MyRpcServer>();//注册服务
                   });

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
            return service;
        }
    }

    public class MyRpcServer : RpcServer
    {
        [Description("登录")]
        [TouchRpc("Login")]
        public bool Login(string account, string password)
        {
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }
    }

    public class MyRpcServer : TransientRpcServer
    {
        [Description("登录")]
        [TouchRpc(MethodFlags = MethodFlags.IncludeCallContext)]//使用调用上才文
        [MyRpcActionFilter]
        public bool Login(ICallContext callContext, string account, string password)
        {
            if (callContext.Caller is TcpTouchRpcSocketClient tcpTouchRpcSocketClient)
            {
                Console.WriteLine(tcpTouchRpcSocketClient.IP);//可以获取到IP
                Console.WriteLine("TcpTouchRpc请求");
            }
            else if (callContext.Caller is HttpTouchRpcSocketClient)
            {
                Console.WriteLine("HttpTouchRpc请求");
            }
            else if (callContext.Caller is UdpCaller)
            {
                Console.WriteLine("UdpTouchRpc请求");
            }

            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }

        [Description("性能测试")]
        [TouchRpc]
        [MyRpcActionFilter]
        public int Performance(int a)
        {
            return a;
        }
    }

    public class MyTouchRpcAttribute : TouchRpcAttribute
    {
        private readonly string m_route;

        public MyTouchRpcAttribute(string route = default)
        {
            this.m_route = route;
        }

        public override string GetInvokenKey(MethodInstance methodInstance)
        {
            if (m_route.IsNullOrEmpty())
            {
                return base.GetInvokenKey(methodInstance);
            }
            return m_route;
        }
    }

    public class MyRpcActionFilterAttribute : RpcActionFilterAttribute, IRpcActionFilter
    {
        public override void Executing(ICallContext callContext, ref InvokeResult invokeResult)
        {
            if (callContext.Caller is TcpTouchRpcSocketClient client)
            {
                client.Logger.Info($"即将执行RPC-{callContext.MethodInstance.Name}");
            }
            base.Executing(callContext, ref invokeResult);
        }

        public override void Executed(ICallContext callContext, ref InvokeResult invokeResult)
        {
            if (callContext.Caller is TcpTouchRpcSocketClient client)
            {
                client.Logger.Info($"执行RPC-{callContext.MethodInstance.Name}完成，状态={invokeResult.Status}");
            }
            base.Executed(callContext, ref invokeResult);
        }

        public override void ExecutException(ICallContext callContext, ref InvokeResult invokeResult, Exception exception)
        {
            if (callContext.Caller is TcpTouchRpcSocketClient client)
            {
                client.Logger.Info($"执行RPC-{callContext.MethodInstance.Name}异常，信息={invokeResult.Message}");
            }

            base.ExecutException(callContext, ref invokeResult, exception);
        }
    }

    internal class MyTouchRpcPlugin : TouchRpcPluginBase
    {
        protected override void OnHandshaking(ITouchRpc client, VerifyOptionEventArgs e)
        {
            //if (e.Metadata["a"] != "a")
            //{
            //    e.IsPermitOperation = false;//不允许连接
            //    e.Message = "元数据不对";//同时返回消息
            //    e.Handled= true;//表示该消息已在此处处理。
            //    return;
            //}
            if (e.Token == "123")
            {
                e.IsPermitOperation = true;
                e.Handled = true;
                return;
            }
            base.OnHandshaking(client, e);
        }
    }
}