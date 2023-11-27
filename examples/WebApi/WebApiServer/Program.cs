using System;
using System.IO;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using TouchSocket.WebApi.Swagger;

namespace WebApiServerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()
               .SetListenIPHosts(7789)
               .ConfigurePlugins(a =>
               {
                   a.UseCheckClear();

                   a.Add<AuthenticationPlugin>();

                   a.UseWebApi()
                   .ConfigureRpcStore(store =>
                   {
                       store.RegisterServer<ApiServer>();//注册服务

#if DEBUG
                       //下列代码，会生成客户端的调用代码。
                       var codeString = store.GetProxyCodes("WebApiProxy", typeof(WebApiAttribute));
                       File.WriteAllText("../../../WebApiProxy.cs", codeString);
#endif
                   });

                   a.UseSwagger()//使用Swagger页面
                   .UseLaunchBrowser();//启动浏览器

                   //此插件是http的兜底插件，应该最后添加。作用是当所有路由不匹配时返回404.且内部也会处理Option请求。可以更好的处理来自浏览器的跨域探测。
                   a.UseDefaultHttpServicePlugin();
               }));
            service.Start();

            Console.WriteLine("以下连接用于测试webApi");
            Console.WriteLine($"使用：http://127.0.0.1:7789/ApiServer/Sum?a=10&b=20");

            Console.ReadKey();
        }
    }

    public class ApiServer : RpcServer
    {
        private readonly ILog m_logger;

        public ApiServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [Origin(AllowOrigin = "*")]//跨域设置
        [Router("[api]/[action]ab")]//此路由会以"/ApiServer/Sumab"实现
        [Router("[api]/[action]")]//此路由会以"/ApiServer/Sum"实现
        [WebApi(HttpMethodType.GET)]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        [WebApi(HttpMethodType.POST)]
        public int TestPost(MyClass myClass)
        {
            return myClass.A + myClass.B;
        }

        /// <summary>
        /// 使用调用上下文，响应文件下载。
        /// </summary>
        /// <param name="callContext"></param>
        [WebApi(HttpMethodType.GET)]
        public Task<string> DownloadFile(IWebApiCallContext callContext, string id)
        {
            if (id == "rrqm")
            {
                callContext.HttpContext.Response.FromFile(@"D:\System\Windows.iso", callContext.HttpContext.Request);
                return Task.FromResult("ok");
            }
            return Task.FromResult("id不正确。");
        }

        /// <summary>
        /// 使用调用上下文，获取实际请求体。
        /// </summary>
        /// <param name="callContext"></param>
        [WebApi(HttpMethodType.POST)]
        [Router("[api]/[action]")]
        public Task<string> PostContent(IWebApiCallContext callContext)
        {
            if (callContext.Caller is ISocketClient socketClient)
            {
                this.m_logger.Info($"IP:{socketClient.IP},Port:{socketClient.Port}");//获取Ip和端口
            }
            if (callContext.HttpContext.Request.TryGetContent(out var content))
            {
                this.m_logger.Info($"共计：{content.Length}");
            }

            return Task.FromResult("ok");
        }
    }

    public class MyClass
    {
        public int A { get; set; }
        public int B { get; set; }
    }

    /// <summary>
    /// 鉴权插件
    /// </summary>
    class AuthenticationPlugin : PluginBase, IHttpPlugin
    {
        public async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            string aut = e.Context.Request.Headers["Authorization"];
            if (aut.IsNullOrEmpty())//授权header为空
            {
               await e.Context.Response
                    .SetStatus(401, "授权失败")
                    .AnswerAsync();
                return;
            }

            //伪代码，假设使用jwt解码成功。那就执行下一个插件。
            //if (jwt.Encode(aut))
            //{
            //   此处可以做一些授权相关的。
            //}
            await e.InvokeNext();
        }
    }
}