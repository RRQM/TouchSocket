using System;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //证书在RRQMBox/Ssl证书相关/证书生成.zip  解压获取。
            //然后放在运行目录。
            //最后客户端需要先安装证书。

            var service = new HttpService();
            service.Setup(new TouchSocketConfig()//加载配置
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger<ConsoleLogger>();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyHttpPlug>();
                }))
                .Start();

            Console.WriteLine("Http服务器已启动");
            Console.WriteLine("访问 http://127.0.0.1:7789/success 返回响应");
            Console.WriteLine("访问 http://127.0.0.1:7789/file 响应文件");
            Console.WriteLine("访问 http://127.0.0.1:7789/html 返回html");
            Console.WriteLine("Post访问 http://127.0.0.1:7789/uploadfile 上传文件");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 支持GET、Post、Put，Delete，或者其他
    /// </summary>
    internal class MyHttpPlug : HttpPluginBase
    {
        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (e.Context.Request.UrlEquals("/success"))
            {
                e.Context.Response.FromText("Success").Answer();//直接回应
                Console.WriteLine("处理完毕");
                e.Handled = true;
            }
            else if (e.Context.Request.UrlEquals("/file"))
            {
                e.Context.Response
                    .SetStatus()//必须要有状态
                    .FromFile(@"D:\System\Windows.iso", e.Context.Request);//直接回应文件。
            }
            else if (e.Context.Request.UrlEquals("/html"))
            {
                StringBuilder stringBuilder = new StringBuilder();
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
                e.Context.Response.Answer();
            }
            base.OnGet(client, e);
        }

        protected override void OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (e.Context.Request.UrlEquals("/uploadfile"))
            {
                try
                {
                    if (e.Context.Request.ContentLen > 1024 * 1024 * 100)//全部数据体超过100Mb则直接拒绝接收。
                    {
                        e.Context.Response
                            .SetStatus("403", "数据过大")
                            .Answer();
                        return;
                    }
                    //此操作会先接收全部数据，然后再分割数据。
                    //所以上传文件不宜过大，不然会内存溢出。
                    var multifileCollection = e.Context.Request.GetMultifileCollection();

                    foreach (var item in multifileCollection)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append($"文件名={item.FileName}\t");
                        stringBuilder.Append($"数据长度={item.Length}");
                        client.Logger.Info(stringBuilder.ToString());
                    }

                    e.Context.Response
                            .SetStatus()
                            .FromText("Ok")
                            .Answer();
                }
                catch (Exception ex)
                {
                    client.Logger.Exception(ex);
                }
            }
            base.OnPost(client, e);
        }
    }
}