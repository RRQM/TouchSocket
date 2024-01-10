using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace WebApiConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddServiceHostedService<IHttpService, HttpService>(config =>
            {
                config.SetListenIPHosts(7789)
                .ConfigureContainer(a =>
                {
                    a.AddRpcStore(store =>
                    {
                        store.RegisterServer<ApiServer>();//ע�����
                    });

                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseCheckClear();

                    a.UseWebApi();

                    a.UseHttpStaticPage()
                    .SetNavigateAction(request =>
                    {
                        //�˴����������ض���
                        return request.RelativeURL;
                    })
                    .SetResponseAction(response =>
                    {
                        //����������Ӧͷ
                    })
                    .AddFolder("api/");//��Ӿ�̬ҳ���ļ��У���ʹ�� http://127.0.0.1:7789/index.html ���ʾ�̬��ҳ

                    //�˲����http�Ķ��ײ����Ӧ�������ӡ������ǵ�����·�ɲ�ƥ��ʱ����404.���ڲ�Ҳ�ᴦ��Option���󡣿��Ը��õĴ�������������Ŀ���̽�⡣
                    a.UseDefaultHttpServicePlugin();
                });
            });

            var host = builder.Build();
            host.Run();
        }
    }

    public partial class ApiServer : RpcServer
    {
        private readonly ILog m_logger;

        public ApiServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [Origin(AllowOrigin = "*")]//��������
        [Router("[api]/[action]ab")]//��·�ɻ���"/Server/Sumab"ʵ��
        [Router("[api]/[action]")]//��·�ɻ���"/Server/Sum"ʵ��
        [WebApi(HttpMethodType.GET)]
        public int Sum(int a, int b)
        {
            m_logger.Info("Sum");
            return a + b;
        }

        [WebApi(HttpMethodType.POST)]
        public int TestPost(MyClass myClass)
        {
            m_logger.Info("TestPost");
            return myClass.A + myClass.B;
        }

    }

    public class MyClass
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}