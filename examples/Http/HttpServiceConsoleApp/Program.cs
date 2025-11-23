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

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace ConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //如果需要创建 https，则需要证书文件，此处提供一个测试证书文件
        //证书在Ssl证书相关/证书生成.zip  解压获取。
        //然后放在运行目录。
        //最后客户端需要先安装证书。

        var service = new HttpService();

        var config = new TouchSocketConfig();
        config.SetListenIPHosts(7789)
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  a.Add<MyHttpPlug1>();
                  a.Add<MyHttpPlug2>();
                  a.Add<MyHttpPlug3>();
                  a.Add<MyHttpPlugin4>();
                  a.Add<MyUploadBigFileHttpPlugin>();
                  a.Add<MyBigWriteHttpPlug>();
                  a.Add<MyCustomDownloadHttpPlug>();
                  a.Add<TestFormPlugin>();
                  a.Add<MyDelayResponsePlugin>();
                  a.Add<MyDelayResponsePlugin2>();

                  //default插件应该最后添加，其作用是
                  //1、为找不到的路由返回404
                  //2、处理 header 为Option的探视跨域请求。
                  a.UseDefaultHttpServicePlugin();
              });

        ConfigureStaticPage(config);

        await service.SetupAsync(new TouchSocketConfig()//加载配置
              );
        await service.StartAsync();

        Console.WriteLine("Http服务器已启动");
        Console.WriteLine("访问 http://127.0.0.1:7789/index.html 访问静态网页");
        Console.WriteLine("访问 http://127.0.0.1:7789/success 返回响应");
        Console.WriteLine("访问 http://127.0.0.1:7789/file 响应文件");
        Console.WriteLine("访问 http://127.0.0.1:7789/html 返回html");
        Console.WriteLine("Post访问 http://127.0.0.1:7789/uploadfile 上传文件");
        Console.ReadKey();
    }

    private static void ConfigureStaticPage(TouchSocketConfig config)
    {
        #region Http服务器启用静态页面插件
        config.ConfigurePlugins(a =>
        {
            a.UseHttpStaticPage(options =>
            {
                //添加静态页面文件夹
                options.AddFolder("api/");

                #region 静态页面请求资源定向
                options.SetNavigateAction(request =>
                {
                    //此处可以设置重定向
                    return request.RelativeURL;
                });
                #endregion

                #region 静态页面响应设置
                options.SetResponseAction(response =>
                {
                    //可以设置响应头
                });
                #endregion

                #region 静态页面配置ContentType
                options.SetContentTypeProvider(provider =>
                {
                    provider.Add(".txt", "text/plain");
                });
                #endregion

            });
        });
        #endregion
    }

    private static async Task CreateHttpService()
    {
        #region 创建Http服务器 {10}
        var service = new HttpService();
        await service.SetupAsync(new TouchSocketConfig()//加载配置
              .SetListenIPHosts(7789)
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  a.Add<MyHttpPlug1>();
                  //default插件应该最后添加，其作用是
                  //1、为找不到的路由返回404
                  //2、处理 header 为Option的探视跨域请求。
                  a.UseDefaultHttpServicePlugin();
              }));
        await service.StartAsync();
        #endregion
    }

    private static async Task CreateHttpService1()
    {
        #region 创建Ssl的Http服务器 {4-8}
        var service = new HttpService();
        await service.SetupAsync(new TouchSocketConfig()//加载配置
              .SetListenIPHosts(7789)
              .SetServiceSslOption(options =>
              {
                  options.Certificate = new X509Certificate2("TouchSocketTestCert.pfx", "123456");
                  options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
              })
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  a.Add<MyHttpPlug1>();
                  //default插件应该最后添加，其作用是
                  //1、为找不到的路由返回404
                  //2、处理 header 为Option的探视跨域请求。
                  a.UseDefaultHttpServicePlugin();
              }));
        await service.StartAsync();
        #endregion
    }
}

public class MyBigWriteHttpPlug : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (e.Context.Request.IsPost())
        {
            if (e.Context.Request.UrlEquals("/bigwrite"))
            {
                try
                {
                    var count = 0L;
                    while (true)
                    {
                        //var buffer = new byte[1024 * 64];

                        using (var blockResult = await e.Context.Request.ReadAsync())
                        {
                            if (blockResult.IsCompleted)
                            {
                                break;
                            }
                            count += blockResult.Memory.Length;
                            Console.WriteLine(blockResult.Memory.Length);
                            //这里可以一直处理读到的数据。
                            //blockResult.Memory.CopyTo(buffer);
                        }
                    }

                    Console.WriteLine($"读取数据，长度={count}");

                    await e.Context.Response
                             .SetStatusWithSuccess()
                             .FromText("Ok")
                             .AnswerAsync();

                }
                catch (Exception ex)
                {
                    client.Logger.Exception(ex);
                }
            }
        }
        await e.InvokeNext();
    }
}

public class MyUploadBigFileHttpPlugin : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (e.Context.Request.IsPost())
        {
            if (e.Context.Request.UrlEquals("/uploadbigfile"))
            {
                try
                {
                    var fileName = e.Context.Request.Headers["FileName"];
                    if (fileName.IsEmpty)
                    {
                        await e.Context.Response
                             .SetStatus(502, "fileName is null")
                             .FromText("fileName is null")
                             .AnswerAsync();
                        return;
                    }
                    using (var stream = File.OpenWrite(fileName))
                    {
                        await e.Context.Request.ReadCopyToAsync(stream);
                    }

                    client.Logger.Info("大文件上传成功");
                    await e.Context.Response
                             .SetStatusWithSuccess()
                             .FromText("Ok")
                             .AnswerAsync();
                }
                catch (Exception ex)
                {
                    client.Logger.Exception(ex);
                }
            }
        }
        await e.InvokeNext();
    }
}

public class MyHttpPlugin4 : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (e.Context.Request.IsPost())
        {
            if (e.Context.Request.UrlEquals("/uploadfile"))
            {
                try
                {
                    //情况1，数据较小一次性获取请求体
                    var content = await e.Context.Request.GetContentAsync();

                    //情况2，当数据太大时，可持续读取
                    #region Http服务器持续读取Body内容
                    while (true)
                    {
                        var buffer = new byte[1024 * 64];

                        using (var blockResult = await e.Context.Request.ReadAsync())
                        {
                            var memory = blockResult.Memory;//本次读取的数据

                            //把本次读取的数据写入缓冲区，或者直接处理
                            memory.CopyTo(buffer);

                            if (blockResult.IsCompleted)
                            {
                                //读取完毕
                                break;
                            }
                        }
                    }
                    #endregion

                    //情况3，或者把数据读到流
                    #region Http服务器获取Body持续写入Stream中
                    using (var stream = new MemoryStream())
                    {
                        //
                        await e.Context.Request.ReadCopyToAsync(stream);
                    }
                    #endregion



                    //情况4，接收小文件。
                    #region Http服务器获取Body小文件集合
                    if (e.Context.Request.ContentLength > 1024 * 1024 * 100)//全部数据体超过100Mb则直接拒绝接收。
                    {
                        await e.Context.Response
                             .SetStatus(403, "数据过大")
                             .AnswerAsync();
                        return;
                    }

                    //此操作会先接收全部数据，然后再分割数据。
                    //所以上传文件不宜过大，不然会内存溢出。
                    var multifileCollection = await e.Context.Request.GetFormCollectionAsync();

                    foreach (var file in multifileCollection.Files)
                    {
                        var stringBuilder = new StringBuilder();
                        stringBuilder.Append($"文件名={file.FileName}\t");
                        stringBuilder.Append($"数据长度={file.Length}");
                        client.Logger.Info(stringBuilder.ToString());

                        //文件数据
                        var data = file.Data;

                        //开始保存数据到磁盘
                        using (var fileStream = File.Create(file.Name))
                        {
                            await fileStream.WriteAsync(data);
                            await fileStream.FlushAsync();
                        }
                    }
                    #endregion


                    await e.Context.Response
                             .SetStatusWithSuccess()
                             .FromText("Ok")
                             .AnswerAsync();
                }
                catch (Exception ex)
                {
                    client.Logger.Exception(ex);
                }
            }
        }
        await e.InvokeNext();
    }
}

public class MyHttpPlug3 : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        #region Http服务器响应页面请求
        if (e.Context.Request.IsGet())
        {
            if (e.Context.Request.UrlEquals("/html"))
            {
                //回应html
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<!DOCTYPE html>");
                stringBuilder.AppendLine("<html>");
                stringBuilder.AppendLine("<head>");
                stringBuilder.AppendLine("<meta charset=\"utf-8\"/>");
                stringBuilder.AppendLine("<title>TouchSocket</title>");
                stringBuilder.AppendLine("</head>");
                stringBuilder.AppendLine("<body>");
                stringBuilder.AppendLine("<div id=\"kuang\" style=\"width: 50%;height: 85%;left: 25%;top:15%;position: absolute;\">");
                stringBuilder.AppendLine("<a id=\"MM\"  style=\"font-size: 30px;font-family: 微软雅黑;width: 100%;\">王二麻子</a>");
                stringBuilder.AppendLine("<input type=\"text\" id=\"NN\" value=\"\" style=\"font-size: 30px;width:100%;position: relative;top: 30px;\"/>");
                stringBuilder.AppendLine("<input type=\"button\" id=\"XX\" value=\"我好\" style=\"font-size: 30px;width: 100%;position: relative;top: 60px;\" onclick=\"javascript:jump()\" />");
                stringBuilder.AppendLine("</div>");
                stringBuilder.AppendLine("</body>");
                stringBuilder.AppendLine("</html>");

                e.Context.Response
                         .SetStatusWithSuccess()//必须要有状态
                         .SetContentTypeByExtension(".html")
                         .SetContent(stringBuilder.ToString());
                await e.Context.Response.AnswerAsync();
                return;
            }
        }
        #endregion
        await e.InvokeNext();
    }
}

#region Http服务器响应文件
public class MyHttpPlug2 : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (e.Context.Request.IsGet())
        {
            if (e.Context.Request.UrlEquals("/file"))
            {
                try
                {
                    //假设这是一个很大的文件
                    var fileInfo = new FileInfo(@"D:\System\Windows.iso");

                    //可以重新命名下载的文件名，不设置则为本地文件名
                    string fileName = null;

                    //是否自动启用gzip压缩，前提是HttpRequest请求了gzip
                    var autoGzip = true;

                    //流式传输操作，可以控制流速、获取当前流速、进度等
                    var httpFlowOperator = new HttpFlowOperator();

                    //流速控制，当前设置为最大512KB/秒
                    httpFlowOperator.MaxSpeed = 1024 * 512;

                    //每次读取文件长度，数值越大，效率越高，但对内存要求也越高
                    httpFlowOperator.BlockSize = 1024 * 8;

                    //可以设置可取消令箭操作
                    using var cts = new CancellationTokenSource();
                    httpFlowOperator.Token = cts.Token;

                    using var timer = new Timer(state =>
                     {
                         //每秒打印当前流速、进度等信息
                         Console.WriteLine($"当前流速：{httpFlowOperator.Speed() / 1024.0}KB/s，进度：{httpFlowOperator.Progress}");
                     }, null, 1000, 1000);

                    //直接回应文件。
                    await e.Context.Response
                          .SetStatusWithSuccess()//必须要有状态
                          .FromFileAsync(fileInfo, httpFlowOperator, e.Context.Request, fileName, autoGzip);
                }
                catch (Exception ex)
                {
                    await e.Context.Response
                        .SetStatus(403, ex.Message)
                        .FromText(ex.Message)
                        .AnswerAsync();
                }

                return;
            }
        }
        await e.InvokeNext();
    }
}
#endregion


#region Http服务器使用插件
public class MyHttpPlug1 : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        #region HttpContext生命周期
        var request = e.Context.Request;//http请求体
        var response = e.Context.Response;//http响应体
        #endregion

        if (request.IsGet() && request.UrlEquals("/success"))
        {
            //直接响应文字
            await response
                 .SetStatus(200, "success")
                 .FromText("Success")
                 .AnswerAsync();//直接回应
            Console.WriteLine("处理/success");
            return;
        }


        //无法处理，调用下一个插件
        await e.InvokeNext();
    }
}
#endregion

public class MyHttpPlug11 : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;//http请求体
        var response = e.Context.Response;//http响应体

        #region Http服务器获取Query参数
        var name = e.Context.Request.Query.Get("name");
        //或者
        var name2 = e.Context.Request.Query["name"];
        //或者
        if (e.Context.Request.Query.TryGetValue("name", out var nameValue))
        {
            //获取成功
        }
        #endregion

        #region Http服务器获取Header参数
        var nameHeader1 = e.Context.Request.Headers.Get("name");
        //或者
        var name2Header1 = e.Context.Request.Headers["name"];
        //或者
        if (e.Context.Request.Headers.TryGetValue("name", out var nameValueHeader1))
        {
            //获取成功
        }
        #endregion

        #region Http服务器从预设获取Header参数
        var nameHeader2 = e.Context.Request.Headers.Get(HttpHeaders.Authorization);
        //或者
        var name2Header2 = e.Context.Request.Headers[HttpHeaders.Authorization];
        //或者
        if (e.Context.Request.Headers.TryGetValue(HttpHeaders.Authorization, out var nameValueHeader2))
        {
            //获取成功
        }
        #endregion

        #region Http服务器获取字符串Body内容
        var bodyString = await e.Context.Request.GetBodyAsync();
        #endregion

        #region Http服务器获取内存Body内容
        var content = await e.Context.Request.GetContentAsync();
        #endregion

        if (request.IsGet() && request.UrlEquals("/success"))
        {
            #region Http服务器设置响应状态
            e.Context.Response.SetStatus(200, "success");

            //或者
            e.Context.Response.SetStatusWithSuccess();
            #endregion

            #region Http服务器设置响应Header
            e.Context.Response.Headers.Add("1", "1");

            //或者
            e.Context.Response.Headers["1"] = "1";

            //或者
            e.Context.Response.Headers.TryAdd("1", "1");

            //或者
            e.Context.Response.AddHeader("1", "1");

            //或者
            e.Context.Response.AddHeader(HttpHeaders.Authorization, "Authorization");
            #endregion

            #region Http服务器设置响应内容
            e.Context.Response.SetContent("Hello World");

            //或者
            e.Context.Response.FromText("Hello World");

            //或者
            e.Context.Response.SetContent(Encoding.UTF8.GetBytes("Hello World"));
            #endregion

            #region Http服务器设置响应Json内容
            var obj = new
            {
                Name = "TouchSocket",
                Author = "若汝棋茗",
                Blog = "https://blog.csdn.net/qq_40374647"
            };
            e.Context.Response.FromJson(JsonConvert.SerializeObject(obj));
            #endregion

            #region Http服务器设置响应Xml内容
            var xml = @"
<?xml version=""1.0"" encoding=""utf-8"" ?>
<root>
    <Name>TouchSocket</Name>
    <Author>若汝棋茗</Author>
    <Blog>https://blog.csdn.net/qq_40374647</Blog>
</root>";
            e.Context.Response.FromXml(xml);
            #endregion

            #region Http服务器执行响应
            //直接响应文字
            await e.Context.Response.AnswerAsync();
            #endregion

            Console.WriteLine("处理/success");
            return;
        }


        //无法处理，调用下一个插件
        await e.InvokeNext();
    }
}
public class TestFormPlugin : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (e.Context.Request.IsPost())
        {
            if (e.Context.Request.UrlEquals("/form"))
            {
                #region Http服务器获取Form参数
                var formCollection = await e.Context.Request.GetFormCollectionAsync();
                foreach (var item in formCollection)
                {
                    Console.WriteLine($"{item.Key}={item.Value}");
                }
                #endregion

                #region Http服务器链式响应
                await e.Context.Response
                         .SetStatusWithSuccess()
                         .FromText("Ok")
                         .AnswerAsync();
                #endregion

            }
        }
        await e.InvokeNext();
    }
}

public class MyCustomDownloadHttpPlug : PluginBase, IHttpPlugin
{
    private readonly ILog m_logger;

    public MyCustomDownloadHttpPlug(ILog logger)
    {
        this.m_logger = logger;
    }
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;//http请求体
        var response = e.Context.Response;//http响应体

        if (request.IsGet() && request.UrlEquals("/CustomDownload"))
        {
            await this.TransferFileToResponse(response, "D:\\迅雷下载\\QQMusic_Setup_2045.exe");
            return;
        }


        //无法处理，调用下一个插件
        await e.InvokeNext();
    }

    public async Task TransferFileToResponse(HttpResponse response, string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                await response.SetStatus(403, $"Object Not Exist")
                    .AnswerAsync();
                return;
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                response.SetContentTypeFromFileName(Path.GetFileName(filePath));
                response.SetStatus(200, "Success");

                var len = fileStream.Length;
                response.ContentLength = len;

                var buffer = new Memory<byte>(new byte[1024 * 512]);
                while (true)
                {
                    var readLen = await fileStream.ReadAsync(buffer);
                    if (readLen == 0)
                    {
                        break;
                    }

                    await response.WriteAsync(buffer[..readLen]);
                }
            }

            this.m_logger.Info("Success");
        }
        catch (Exception ex)
        {
            this.m_logger.Exception(ex);
            //await response.SetStatus(403, ex.Message)
            //    .AnswerAsync();
        }

    }
}

internal class MyDelayResponsePlugin : PluginBase, IHttpPlugin
{
    private readonly ConcurrentQueue<TaskCompletionSource<string>> m_queue = new();

    public MyDelayResponsePlugin()
    {
        Task.Factory.StartNew(this.HandleQueue, TaskCreationOptions.LongRunning);
    }

    private async Task HandleQueue()
    {
        while (true)
        {
            //模拟延迟
            await Task.Delay(3000);

            //处理队列
            while (this.m_queue.TryDequeue(out var tcs))
            {
                //返回结果
                tcs.SetResult(Guid.NewGuid().ToString());
            }
        }
    }

    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;//http请求体
        var response = e.Context.Response;//http响应体
        if (request.UrlEquals("/delay"))
        {
            var tcs = new TaskCompletionSource<string>();
            this.m_queue.Enqueue(tcs);
            var result = await tcs.Task;

            await response
                .SetStatus(200, "success")
                .FromText(result)
                .AnswerAsync();
        }
        await e.InvokeNext();
    }
}

internal class MyDelayResponsePlugin2 : PluginBase, IHttpPlugin
{
    private readonly ConcurrentQueue<MyTaskCompletionSource> m_queue = new();

    public MyDelayResponsePlugin2()
    {
        Task.Factory.StartNew(this.HandleQueue, TaskCreationOptions.LongRunning);
    }

    private async Task HandleQueue()
    {
        while (true)
        {
            //模拟延迟
            await Task.Delay(3000);

            //处理队列
            while (this.m_queue.TryDequeue(out var tcs))
            {
                //返回结果

                var client = tcs.Client;
                var response = tcs.Context.Response;

                response.SetStatus(200, "success");
                response.IsChunk = true;

                for (var i = 0; i < 5; i++)
                {
                    await response.WriteAsync(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + "\r\n"));
                    await Task.Delay(1000);
                }

                await response.CompleteChunkAsync();

                tcs.SetResult(true);
            }
        }
    }

    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;//http请求体
        var response = e.Context.Response;//http响应体
        if (request.UrlEquals("/delay2"))
        {
            var tcs = new MyTaskCompletionSource(client, e.Context);
            this.m_queue.Enqueue(tcs);
            var result = await tcs.Task;
            //不做任何处理
        }
        await e.InvokeNext();
    }

    private class MyTaskCompletionSource : TaskCompletionSource<bool>
    {
        public MyTaskCompletionSource(IHttpSessionClient client, HttpContext context)
        {
            this.Client = client;
            this.Context = context;
        }

        public IHttpSessionClient Client { get; }
        public HttpContext Context { get; }
    }
}

#region Http服务器响应有长度大数据 {10,13,18}
internal class MyHttpPlugin22 : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;//http请求体
        var response = e.Context.Response;//http响应体
        if (e.Context.Request.IsGet() && e.Context.Request.UrlEquals("/data1"))
        {
            //1.先设置需要响应的相关配置
            response.SetStatusWithSuccess();

            //2.然后设置数据总长度
            response.ContentLength = 1024 * 1024;

            for (var i = 0; i < 1024; i++)
            {
                //3.将数据持续写入
                await response.WriteAsync(new byte[1024]);
            }
        }
        await e.InvokeNext();
    }
}
#endregion

#region Http服务器响应无长度大数据 {10,13,18,22}
internal class MyHttpPlugin33 : PluginBase, IHttpPlugin
{
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;//http请求体
        var response = e.Context.Response;//http响应体
        if (e.Context.Request.IsGet() && e.Context.Request.UrlEquals("/data2"))
        {
            //1.先设置需要响应的相关配置
            response.SetStatusWithSuccess();

            //2.设置使用Chunk模式
            response.IsChunk = true;

            for (var i = 0; i < 1024; i++)
            {
                //3.将数据持续写入
                await response.WriteAsync(new byte[1024]);
            }

            //4.在正式数据传输完成后，调用此方法，客户端才知道数据结束了
            await response.CompleteChunkAsync();
        }
        await e.InvokeNext();
    }
}
#endregion
