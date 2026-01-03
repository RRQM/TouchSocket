using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace HttpSseConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        #region HttpSse创建服务器
        var service = new HttpService();
        await service.SetupAsync(new TouchSocketConfig()//加载配置
             .SetListenIPHosts(7789)
        .ConfigurePlugins(a =>
        {
            a.Add<SsePlugin>();
            a.UseDefaultHttpServicePlugin();
        })
             );
        await service.StartAsync();

        Console.WriteLine("Http服务器已启动，按任意键sse请求");
        #endregion

        Console.ReadKey();

        await RequestSse();

        Console.WriteLine("按任意键退出");
        Console.ReadKey();
    }

    private static async Task RequestSse()
    {
        #region HttpSse客户端请求
        using var client = new TouchSocket.Http.HttpClient();
        await client.ConnectAsync("127.0.0.1:7789");
        var httpRequest = new HttpRequest();
        httpRequest.InitHeaders()
            .SetUrl("/sse")
            .AsGet();
        using var cancellationTokenSource = new CancellationTokenSource(1000 * 30);
        using var responseResult = await client.RequestAsync(httpRequest, cancellationTokenSource.Token);
        var response = responseResult.Response;
        var sseReader = response.CreateSseReader();
        int count = 0;
        await foreach (var sse in sseReader.ReadAllAsync(cancellationTokenSource.Token))
        {
            Console.WriteLine($"Count:{count},Id:{sse.EventId},Event:{sse.EventType},Data:{sse.Data}");
            count++;
        }
        #endregion
    }
}

internal class SsePlugin : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;
        var response = e.Context.Response;


        if (request.IsGet())
        {
            if (request.UrlEquals("/sse"))
            {
                #region HttpSse发送消息
                // 设置正确响应码和头部
                response.SetStatusWithSuccess();

                // 创建SSE写入器
                var sseWriter = response.CreateSseWriter();
                for (int i = 0; i < 10; i++)
                {
                    var sseItem = new SseMessage()
                    {
                        Data = $"当前服务器时间: {DateTime.Now:HH:mm:ss}",
                        EventId = i.ToString(),
                        ReconnectionInterval = TimeSpan.FromSeconds(i),
                    };

                    // 发送SSE消息
                    await sseWriter.WriteAsync(sseItem);
                    await Task.Delay(1000);
                }

                // 完成SSE流
                await sseWriter.CompleteAsync();
                #endregion
            }
        }
        await e.InvokeNext();
    }
}