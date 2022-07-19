using System;
using System.ComponentModel;
using System.Linq;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.TouchRpc.Plugins;
using TouchSocket.Sockets;

namespace TouchRpcServerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RpcStore rpcStore = new RpcStore(new TouchSocket.Core.Dependency.Container());
            rpcStore.ProxyUrl = "/proxy";//代理url
            rpcStore.OnRequestProxy = (request) =>//此处做请求验证，保证代理文件安全。
            {
                if (request.TryGetQuery("token", out string value) && value == "123")
                {
                    return true;
                }
                return false;
            };

            rpcStore.AddRpcParser("tcpTouchRpcParser", CreateTcpParser());
            rpcStore.AddRpcParser("httpTouchRpcParser", CreateHttpParser());
            rpcStore.AddRpcParser("udpTouchRpcParser", CreateUdpParser());

            //注册当前程序集的所有服务
            rpcStore.RegisterServer<MyRpcServer>();

            //分享代理，代理文件可通过RRQMTool，或者浏览器获取，远程获取。
            //http://127.0.0.1:8848/proxy?proxy=all&token=123
            rpcStore.ShareProxy(new IPHost(8848));

            //或者直接本地导出代理文件。
            ServerCellCode[] codes = rpcStore.GetProxyInfo(RpcStore.ProxyAttributeMap.Values.ToArray());
            string codeString = CodeGenerator.ConvertToCode("RRQMProxy", codes);
            Console.WriteLine(codeString);

            Console.WriteLine("服务器已启动");
            Console.ReadKey();
        }

        /// <summary>
        /// 解析器是实际的逻辑服务器，此处是<see cref="TcpTouchRpcService"/>。
        /// </summary>
        /// <returns></returns>
        private static IRpcParser CreateTcpParser()
        {
            return new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .SetMaxCount(10000)
                   .SetThreadCount(100)
                   .SetVerifyToken("TouchRpc")
                   .BuildWithTcpTouchRpcService();//此处build相当于new TcpTouchRpcService，然后Setup，然后Start。
        }

        /// <summary>
        /// 此处是<see cref="HttpTouchRpcService"/>。
        /// </summary>
        /// <returns></returns>
        private static IRpcParser CreateHttpParser()
        {
            return new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7790) })
                   .SetMaxCount(10000)
                   .SetThreadCount(100)
                   .SetVerifyToken("TouchRpc")
                   .BuildWithHttpTouchRpcService();//此处build相当于new HttpTouchRpcService，然后Setup，然后Start。
        }

        /// <summary>
        /// 此处是<see cref="UdpTouchRpc"/>。
        /// </summary>
        /// <returns></returns>
        private static IRpcParser CreateUdpParser()
        {
            TouchSocketConfig config = new TouchSocketConfig();
            return config.SetBindIPHost(7791)
                 .SetBufferLength(1024 * 10)
                 .SetThreadCount(10)
                 .BuildWithUdpTouchRpc();
        }
    }

    public class MyRpcServer : RpcServer
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

    internal class MyRpcActionFilterAttribute : Attribute, IRpcActionFilter
    {
        /// <summary>
        /// 在执行RPC之前。
        /// <para>当<paramref name="invokeResult"/>的InvokeStatus不为<see cref="InvokeStatus.Ready"/>。则不会执行RPC</para>
        /// <para>同时，当<paramref name="invokeResult"/>的InvokeStatus为<see cref="InvokeStatus.Success"/>。会直接返回结果</para>
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="invokeResult"></param>
        /// <param name="methodInstance"></param>
        /// <param name="ps"></param>
        public void Executing(object caller, ref InvokeResult invokeResult, MethodInstance methodInstance, object[] ps)
        {
            if (caller is TcpTouchRpcSocketClient client)
            {
                client.Logger.Message($"即将执行RPC-{methodInstance.Name}");
            }
        }

        /// <summary>
        /// 成功执行后。
        /// <para>如果修改<paramref name="invokeResult"/>的InvokeStatus，或Result。则会影响RPC最终结果</para>
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="invokeResult"></param>
        /// <param name="methodInstance"></param>
        public void Executed(object caller, ref InvokeResult invokeResult, MethodInstance methodInstance)
        {
            if (caller is TcpTouchRpcSocketClient client)
            {
                client.Logger.Message($"执行RPC-{methodInstance.Name}完成，状态={invokeResult.Status}");
            }
        }

        /// <summary>
        /// 执行遇见异常。
        /// <para>如果修改<paramref name="invokeResult"/>的InvokeStatus，或Result。则会影响RPC最终结果</para>
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="invokeResult"></param>
        /// <param name="methodInstance"></param>
        /// <param name="exception"></param>
        public void ExecutException(object caller, ref InvokeResult invokeResult, MethodInstance methodInstance, Exception exception)
        {
            if (caller is TcpTouchRpcSocketClient client)
            {
                client.Logger.Message($"执行RPC-{methodInstance.Name}异常，信息={invokeResult.Message}");
            }
        }
    }

    internal class MyTouchRpcPlugin : TouchRpcPluginBase
    {
        public override void OnHandshaking(ITouchRpc client, VerifyOptionEventArgs e)
        {
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