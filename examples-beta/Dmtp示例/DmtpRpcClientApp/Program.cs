using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace DmtpClientApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var service = CreateTcpDmtpService(7789);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static TcpDmtpService CreateTcpDmtpService(int port)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(port)
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc()
                       .ConfigureRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();//注册服务
#if DEBUG
                           File.WriteAllText("../../../RpcProxy.cs", store.GetProxyCodes("RpcProxy", new Type[] { typeof(DmtpRpcAttribute) }));
#endif
                       });
                       //a.Add<MyDmtpPlugin>();
                   })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .SetVerifyToken("Rpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
            return service;
        }
    }


    public class MyRpcServer : TransientRpcServer
    {
        [Description("登录")]
        [DmtpRpc(MethodInvoke = true, MethodFlags = MethodFlags.IncludeCallContext)]//使用调用上才文
        [MyRpcActionFilter]
        public bool Login(ICallContext callContext, string account, string password)
        {
            if (callContext.Caller is TcpDmtpSocketClient socketClient)
            {
                Console.WriteLine(socketClient.IP);//可以获取到IP
                Console.WriteLine("Tcp Rpc请求");
            }
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }

        [Description("注册")]
        [DmtpRpc(MethodInvoke = true)]
        [MyRpcActionFilter]
        public bool Register(RegisterModel register)
        {
            return true;
        }

        [Description("性能测试")]
        [DmtpRpc(MethodInvoke = true)]
        [MyRpcActionFilter]
        public int Performance(int a)
        {
            return a;
        }
    }

    public class RegisterModel
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public int Id { get; set; }
    }

    public class MyDmtpAttribute : DmtpRpcAttribute
    {
        private readonly string m_route;

        public MyDmtpAttribute(string route = default)
        {
            this.m_route = route;
        }

        public override string GetInvokenKey(MethodInstance methodInstance)
        {
            if (this.m_route.IsNullOrEmpty())
            {
                return base.GetInvokenKey(methodInstance);
            }
            return this.m_route;
        }
    }

    public class MyRpcActionFilterAttribute : RpcActionFilterAttribute
    {
        public override void Executing(ICallContext callContext, object[] parameters, ref InvokeResult invokeResult)
        {
            //invokeResult = new InvokeResult()
            //{
            //    Status = InvokeStatus.UnEnable,
            //    Message = "不允许执行",
            //    Result = default
            //};
            if (callContext.Caller is TcpDmtpSocketClient client)
            {
                client.Logger.Info($"即将执行RPC-{callContext.MethodInstance.Name}");
            }
            base.Executing(callContext, parameters, ref invokeResult);
        }

        public override void Executed(ICallContext callContext, object[] parameters, ref InvokeResult invokeResult)
        {
            if (callContext.Caller is TcpDmtpSocketClient client)
            {
                client.Logger.Info($"执行RPC-{callContext.MethodInstance.Name}完成，状态={invokeResult.Status}");
            }
            base.Executed(callContext, parameters, ref invokeResult);
        }

        public override void ExecutException(ICallContext callContext, object[] parameters, ref InvokeResult invokeResult, Exception exception)
        {
            if (callContext.Caller is TcpDmtpSocketClient client)
            {
                client.Logger.Info($"执行RPC-{callContext.MethodInstance.Name}异常，信息={invokeResult.Message}");
            }

            base.ExecutException(callContext, parameters, ref invokeResult, exception);
        }
    }
}