//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        private static async Task Main()
        {
            var service =await CreateTcpDmtpService(7789);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static async Task<TcpDmtpService> CreateTcpDmtpService(int port)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//����
                   .SetListenIPHosts(port)
                   .ConfigureContainer(a =>
                   {
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();//ע�����
#if DEBUG
                           File.WriteAllText("../../../RpcProxy.cs", store.GetProxyCodes("RpcProxy", new Type[] { typeof(DmtpRpcAttribute) }));
#endif
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.Add(typeof(IDmtpHandshakingPlugin), async (c, e) =>
                       {
                           await e.InvokeNext();
                       });
                       a.Add(typeof(IDmtpHandshakedPlugin), async (c, e) =>
                       {
                           await e.InvokeNext();
                       });
                       a.UseDmtpRpc();
                       //a.Add<MyDmtpPlugin>();
                   })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Rpc"
                   });

            await service.SetupAsync(config);
            await service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}�������������˿ڣ�{port}");
            return service;
        }
    }

    public partial class MyRpcServer : TransientRpcServer
    {
        [Description("��¼")]
        [DmtpRpc(MethodInvoke = true)]//ʹ�õ����ϲ���
        [MyRpcActionFilter]
        public bool Login(ICallContext callContext, string account, string password)
        {
            if (callContext.Caller is TcpDmtpSessionClient socketClient)
            {
                Console.WriteLine(socketClient.IP);//���Ի�ȡ��IP
                Console.WriteLine("Tcp Rpc����");
            }
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }

        [Description("ע��")]
        [DmtpRpc(MethodInvoke = true)]
        [MyRpcActionFilter]
        public bool Register(RegisterModel register)
        {
            return true;
        }

        [Description("���ܲ���")]
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

    /// <summary>
    /// 3.1版本中已经不需要了
    /// </summary>
    //public class MyDmtpAttribute : DmtpRpcAttribute
    //{
    //    private readonly string m_route;

    //    public MyDmtpAttribute(string route = default)
    //    {
    //        this.m_route = route;
    //    }

    //    public override string GetInvokeKey(RpcMethod methodInstance)
    //    {
    //        if (this.m_route.IsNullOrEmpty())
    //        {
    //            return base.GetInvokeKey(methodInstance);
    //        }
    //        return this.m_route;
    //    }
    //}

    public class MyRpcActionFilterAttribute : RpcActionFilterAttribute
    {
        public override Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
        {
            //invokeResult = new InvokeResult()
            //{
            //    Status = InvokeStatus.UnEnable,
            //    Message = "������ִ��",
            //    Result = default
            //};
            if (callContext.Caller is ISessionClient client)
            {
                client.Logger.Info($"����ִ��Rpc-{callContext.RpcMethod.Name}");
            }
            return Task.FromResult(invokeResult);
        }

        public override Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
        {
            if (callContext.Caller is ISessionClient client)
            {
                if (exception == null)
                {
                    client.Logger.Info($"ִ��RPC-{callContext.RpcMethod.Name}��ɣ�״̬={invokeResult.Status}");
                }
                else
                {
                    client.Logger.Info($"ִ��RPC-{callContext.RpcMethod.Name}�쳣����Ϣ={invokeResult.Message}");
                }

            }
            return Task.FromResult(invokeResult);
        }
    }
}