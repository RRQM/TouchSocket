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
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace ClientConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var consoleAction = new ConsoleAction();

        consoleAction.Add("1", "Get字符串", GetString);
        consoleAction.Add("2", "Get数组", GetBytesArray);
        consoleAction.Add("3", "Get文件", GetFile);
        consoleAction.Add("4", "自定义请求", Request1);
        consoleAction.Add("5", "自定义请求，并持续读取", Request2);
        consoleAction.Add("6", "自定义请求，并持续写入", BigWrite);
        consoleAction.Add("7", "自定义Post请求，并上传流数据", UploadStream);

        consoleAction.ShowAll();

        await consoleAction.RunCommandLineAsync();
    }

    private static async Task CreateClient()
    {
        var config = new TouchSocketConfig();
        #region Http简单设置Ssl远程服务器地址
        config.SetRemoteIPHost("https://touchsocket.net/");
        #endregion

        #region Http客户端启用断线重连
        config.ConfigurePlugins(a =>
        {
            a.UseReconnection<HttpClient>(options =>
            {
                options.PollingInterval = TimeSpan.FromSeconds(1);
            });
        });
        #endregion

        #region Http客户端配置代理
        config.SetProxy(new WebProxy()
        {
            Address = new Uri("http://127.0.0.1"),
            //其他属性自行设置
        });
        #endregion

        #region Http客户端配置系统代理
        config.SetSystemProxy();
        #endregion

        await Task.CompletedTask;
    }

    private static async Task CreateClient2()
    {
        #region 简单创建HttpClient
        var client = new HttpClient();
        await client.ConnectAsync("http://localhost:7219");//直接连接
        #endregion


        await Task.CompletedTask;
    }

    private static async Task UploadStream()
    {
        using var client = await GetHttpClient();
        #region Http客户端流式上传文件
        using (var stream = File.OpenRead("TouchSocket.dll"))
        {
            //创建一个流操作器，可以监视流的读写进度，速度等。
            var flowOperator = new HttpFlowOperator();

            flowOperator.MaxSpeed = 1024 * 100;//最大速度100KB/s
            flowOperator.BlockSize = 1024 * 64;//每次读取64KB，数值越大，效率越高，但也会增加内存消耗。

            //设置10秒超时取消。
            using var cts = new CancellationTokenSource(1000 * 10);
            flowOperator.Token = cts.Token;

            using var timer = new Timer(state =>
            {
                Console.WriteLine($"速度{flowOperator.Speed() / 1024}KB/s，进度{flowOperator.Progress}");
            }, null, 1000, 1000);

            //创建一个请求
            var request = new HttpRequest();
            request.SetContent(new StreamHttpContent(stream, flowOperator));//设置流内容
            request.InitHeaders()
                .SetUrl("/bigwrite")
                .SetHost(client.RemoteIPHost.Host)
                .AsPost();

            using (var responseResult = await client.RequestAsync(request, cts.Token))
            {
                var response = responseResult.Response;
            }
            Console.WriteLine("完成");

        }
        #endregion

    }

    private static async Task BigWrite()
    {
        var client = await GetHttpClient();

        //创建一个请求
        var request = new HttpRequest();
        request.SetContent(new BigDataHttpContent());
        request.InitHeaders()
            .SetUrl("/bigwrite")
            .SetHost(client.RemoteIPHost.Host)
            .AsPost();

        var cts = new CancellationTokenSource(1000 * 10);
        using (var responseResult = await client.RequestAsync(request, cts.Token))
        {
            var response = responseResult.Response;
        }
        Console.WriteLine("完成");
    }

    private static async Task Request2()
    {
        var client = await GetHttpClient();
        #region Http客户端构建自定义请求并持续读取
        //创建一个请求
        var request = new HttpRequest();
        request.InitHeaders()
            .SetUrl("/WeatherForecast")
            .SetHost(client.RemoteIPHost.Host)
            .AsGet();

        var cts = new CancellationTokenSource(1000 * 10);
        using (var responseResult = await client.RequestAsync(request, cts.Token))
        {
            var response = responseResult.Response;

            while (true)
            {
                using (var blockResult = await response.ReadAsync())
                {
                    if (blockResult.IsCompleted)
                    {
                        //数据读完成
                        break;
                    }

                    //每次读到的数据
                    var memory = blockResult.Memory;
                    Console.WriteLine(memory.Length);
                }
            }
        }
        #endregion
    }

    private static async Task Request1()
    {
        using var client = await GetHttpClient();
        #region Http客户端构建基础自定义请求
        //创建一个请求
        var request = new HttpRequest();
        request.InitHeaders()
            .AddHeader("Authorization", "Bearer your-token")
            .SetUrl("/WeatherForecast")
            .SetHost(client.RemoteIPHost.Host)
            .AsGet();

        var cts = new CancellationTokenSource(1000 * 10);
        using (var responseResult = await client.RequestAsync(request, cts.Token))
        {
            var response = responseResult.Response;
            Console.WriteLine(await response.GetBodyAsync());//将接收的数据，一次性转为utf8编码的字符串
        }
        #endregion

    }

    private static async Task PostJson()
    {
        using var client = await GetHttpClient();

        #region Http通过Post发送Json数据

        //直接发起一个Post请求，然后返回Body字符串。

        var jsonData = "{ \"name\": \"张三\", \"age\": 18 }";

        var request = new HttpRequest();
        request.SetContent(new StringHttpContent(jsonData, Encoding.UTF8, "application/json"))
            .InitHeaders()
            .SetUrl("/api/users")
            .SetHost(client.RemoteIPHost.Host)
            .AsPost();

        //创建一个超时操作，10秒后取消。
        using var cts = new CancellationTokenSource(1000 * 10);

        using (var responseResult = await client.RequestAsync(request, cts.Token))
        {
            var response = responseResult.Response;
            var result = await response.GetBodyAsync();
            Console.WriteLine($"响应结果：{result}");
        }
        #endregion

    }
    private static async Task GetString()
    {
        using var client = await GetHttpClient();

        #region Http通过Get获取字符串
        //创建一个超时操作，10秒后取消。
        using var cts = new CancellationTokenSource(1000 * 10);

        //直接发起一个Get请求，然后返回Body字符串。
        var body = await client.GetStringAsync("/WeatherForecast", cts.Token);
        #endregion

    }

    private static async Task GetFile()
    {
        using var client = await GetHttpClient();

        #region Http客户端获取流数据响应
        //创建一个流操作器，可以监视流的读写进度，速度等。
        var flowOperator = new HttpFlowOperator();

        flowOperator.MaxSpeed = 1024 * 100;//最大速度100KB/s
        flowOperator.BlockSize = 1024 * 64;//每次读取64KB，数值越大，效率越高，但也会增加内存消耗。

        //设置10秒超时取消。
        using var cts = new CancellationTokenSource(1000 * 10);
        flowOperator.Token = cts.Token;

        using var timer = new Timer(state =>
        {
            Console.WriteLine($"速度{flowOperator.Speed() / 1024}KB/s，进度{flowOperator.Progress}");
        }, null, 1000, 1000);

        using (var stream = File.Create("1.txt"))
        {
            //直接发起一个Get请求文件，然后写入到流中。
            await client.GetFileAsync("/WeatherForecast", stream, flowOperator);
        }
        #endregion
    }

    private static async Task GetBytesArray()
    {
        using var client = await GetHttpClient();

        #region Http客户端获取字节数组响应
        //创建一个超时操作，10秒后取消。
        using var cts = new CancellationTokenSource(1000 * 10);
        //直接发起一个Get请求，然后返回Body数组。
        var bodyBytes = await client.GetByteArrayAsync("/WeatherForecast", cts.Token);
        #endregion

    }

    private static async Task<HttpClient> GetHttpClient()
    {
        #region 常规配置创建Http客户端
        var client = new HttpClient();

        var config = new TouchSocketConfig();
        #region Http设置远程服务器地址
        config.SetRemoteIPHost("http://127.0.0.1:7789");
        #endregion

        #region Http客户端获取断线通知
        config.ConfigurePlugins(a =>
        {
            a.AddTcpClosedPlugin(async (c, e) =>
            {
                Console.WriteLine("客户端断开连接");
                await e.InvokeNext();
            });
        });
        #endregion

        //配置config
        await client.SetupAsync(config);
        await client.ConnectAsync();//先做连接
        #endregion


        return client;
    }
}

internal class BigDataHttpContent : HttpContent
{
    private readonly long count = 10000;
    private readonly long bufferLength = 1000000;

    protected override bool OnBuildingContent<TByteBlock>(ref TByteBlock byteBlock)
    {
        return false;
    }

    protected override void OnBuildingHeader(IHttpHeader header)
    {
        //header.Add(HttpHeaders.ContentLength, (this.count * this.bufferLength).ToString());
    }

    protected override bool TryComputeLength(out long length)
    {
        length = this.count * this.bufferLength;
        return true;
    }


    protected override async Task WriteContent(PipeWriter writer, CancellationToken cancellationToken)
    {
        var buffer = new byte[this.bufferLength];
        for (var i = 0; i < this.count; i++)
        {
            await writer.WriteAsync(buffer, cancellationToken);
            //Console.WriteLine(i);
        }
    }
}