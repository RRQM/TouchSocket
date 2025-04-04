using System.Text.Json.Serialization;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using TouchSocket.WebApi.Swagger;

namespace WebApiConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.ConfigureContainer(a => 
            {
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<ApiServer>();//ע�����
                });

                a.AddAspNetCoreLogger();
            });

            builder.Services.AddServiceHostedService<IHttpService, HttpService>(config =>
            {
                config.SetListenIPHosts(7789)
                .ConfigurePlugins(a =>
                {
                    a.UseCheckClear();

                    a.UseWebApi()
                    .ConfigureConverter(converter =>
                    {
                        converter.Clear();
                        converter.AddSystemTextJsonSerializerFormatter(options => 
                        {
                            options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                        });
                    });

                    //a.UseSwagger()
                    //.UseLaunchBrowser();

                    //a.UseHttpStaticPage()
                    //.SetNavigateAction(request =>
                    //{
                    //    //�˴����������ض���
                    //    return request.RelativeURL;
                    //})
                    //.SetResponseAction(response =>
                    //{
                    //    //����������Ӧͷ
                    //})
                    //.AddFolder("api/");//��Ӿ�̬ҳ���ļ��У���ʹ�� http://127.0.0.1:7789/index.html ���ʾ�̬��ҳ

                    ////�˲����http�Ķ��ײ����Ӧ�������ӡ������ǵ�����·�ɲ�ƥ��ʱ����404.���ڲ�Ҳ�ᴦ��Option���󡣿��Ը��õĴ�������������Ŀ���̽�⡣
                    //a.UseDefaultHttpServicePlugin();
                });
            });

            var host = builder.Build();
            host.Run();
        }
    }


    [JsonSerializable(typeof(MyClass))]
    [JsonSerializable(typeof(MySum))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }

    public partial class ApiServer : RpcServer
    {
        private readonly ILog m_logger;

        public ApiServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [Router("[api]/[action]ab")]//��·�ɻ���"/Server/Sumab"ʵ��
        [Router("[api]/[action]")]//��·�ɻ���"/Server/Sum"ʵ��
        [WebApi(Method = HttpMethodType.Get)]
        public int Sum(int a, int b)
        {
            //m_logger.Info("Sum");
            return a + b;
        }

        [WebApi(Method = HttpMethodType.Post)]
        public MySum TestPost(MyClass myClass)
        {
            m_logger.Info("TestPost");
            return new MySum() { A = myClass.A, B = myClass.B, Sum = myClass.A + myClass.B };
        }

    }

    public class MyClass
    {
        public int A { get; set; }
        public int B { get; set; }
    }

    public class MySum : MyClass
    {
        public int Sum { get; set; }
    }
}