using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //如果需要创建https，则需要证书文件，此处提供一个测试证书文件
            //证书在Ssl证书相关/证书生成.zip  解压获取。
            //然后放在运行目录。
            //最后客户端需要先安装证书。

            var service = new HttpService();
            service.SetupAsync(new TouchSocketConfig()//加载配置
                .SetListenIPHosts(7789)
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyHttpPlug1>();
                    a.Add<MyHttpPlug2>();
                    a.Add<MyHttpPlug3>();
                    a.Add<MyHttpPlug4>();

                    a.UseHttpStaticPage()
                    .SetNavigateAction(request =>
                    {
                        //此处可以设置重定向
                        return request.RelativeURL;
                    })
                    .SetResponseAction(response =>
                    {
                        //可以设置响应头
                    })
                    .AddFolder("api/");//添加静态页面文件夹

                    //default插件应该最后添加，其作用是
                    //1、为找不到的路由返回404
                    //2、处理header为Option的探视跨域请求。
                    a.UseDefaultHttpServicePlugin();
                }));
            service.StartAsync();

            Console.WriteLine("Http服务器已启动");
            Console.WriteLine("访问 http://127.0.0.1:7789/index.html 访问静态网页");
            Console.WriteLine("访问 http://127.0.0.1:7789/success 返回响应");
            Console.WriteLine("访问 http://127.0.0.1:7789/file 响应文件");
            Console.WriteLine("访问 http://127.0.0.1:7789/html 返回html");
            Console.WriteLine("Post访问 http://127.0.0.1:7789/uploadfile 上传文件");
            Console.ReadKey();
        }
    }

    public class MyHttpPlug4 : PluginBase, IHttpPlugin
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
                        var content =await e.Context.Request.GetContentAsync();

                        //情况2，当数据太大时，可持续读取
                        while (true)
                        {
                            var buffer = new byte[1024 * 64];

                            using (var blockResult = await e.Context.Request.ReadAsync())
                            {
                                if (blockResult.IsCompleted)
                                {
                                    break;
                                }

                                //这里可以一直处理读到的数据。
                                blockResult.Memory.CopyTo(buffer);
                            }
                        }

                        //情况3，或者把数据读到流
                        using (var stream =new MemoryStream())
                        {
                            //
                            await e.Context.Request.ReadCopyToAsync(stream);
                        }


                        //情况4，接收小文件。

                        if (e.Context.Request.ContentLength > 1024 * 1024 * 100)//全部数据体超过100Mb则直接拒绝接收。
                        {
                           await e.Context.Response
                                .SetStatus(403, "数据过大")
                                .AnswerAsync();
                            return;
                        }

                        //此操作会先接收全部数据，然后再分割数据。
                        //所以上传文件不宜过大，不然会内存溢出。
                        var multifileCollection = e.Context.Request.GetMultifileCollection();

                        foreach (var item in multifileCollection)
                        {
                            var stringBuilder = new StringBuilder();
                            stringBuilder.Append($"文件名={item.FileName}\t");
                            stringBuilder.Append($"数据长度={item.Length}");
                            client.Logger.Info(stringBuilder.ToString());
                        }

                       await e.Context.Response
                                .SetStatus()
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
                             .SetStatus()//必须要有状态
                             .SetContentTypeByExtension(".html")
                             .SetContent(stringBuilder.ToString());
                   await e.Context.Response.AnswerAsync();
                    return;
                }
            }

            await e.InvokeNext();
        }
    }

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
                        //直接回应文件。
                      await  e.Context.Response
                            .SetStatus()//必须要有状态
                            .FromFileAsync(@"D:\System\Windows.iso", e.Context.Request);
                    }
                    catch (Exception ex)
                    {
                        await e.Context.Response.SetStatus(403, ex.Message).FromText(ex.Message).AnswerAsync();
                    }

                    return;
                }
            }
            await e.InvokeNext();
        }
    }

    public class MyHttpPlug1 : PluginBase, IHttpPlugin
    {
        public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.IsGet())
            {
                if (e.Context.Request.UrlEquals("/success"))
                {
                    //直接响应文字
                    await e.Context.Response.FromText("Success").AnswerAsync();//直接回应
                    Console.WriteLine("处理完毕");
                    return;
                }
            }

            //无法处理，调用下一个插件
            await e.InvokeNext();
        }
    }
}