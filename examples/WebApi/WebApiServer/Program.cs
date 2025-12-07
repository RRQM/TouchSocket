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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using TouchSocket.WebApi.Swagger;

namespace WebApiServerApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = new HttpService();
        await service.SetupAsync(new TouchSocketConfig()
             .SetListenIPHosts(7789)
             .ConfigureContainer(a =>
             {
                 a.AddRpcStore(store =>
                 {
                     store.RegisterServer<ApiServer>();//注册服务

#if DEBUG
                     #region WebApi服务端代码生成
                     //下列代码，会生成客户端的调用代码。
                     var codeString = store.GetProxyCodes("WebApiProxy", typeof(WebApiAttribute));
                     File.WriteAllText("../../../WebApiProxy.cs", codeString);
                     #endregion
#endif
                 });

                 //添加跨域服务
                 //webapi中使用跨域时，可以不使用插件的UseCors。直接使用RpcFilter的Aop特性完成。即
                 a.AddCors(corsOption =>
                 {
                     //添加跨域策略，后续使用policyName即可应用跨域策略。
                     corsOption.Add("cors", corsBuilder =>
                     {
                         corsBuilder.AllowAnyMethod()
                             .AllowAnyOrigin();
                     });
                 });

                 a.AddLogger(logger =>
                 {
                     logger.AddConsoleLogger();
                     logger.AddFileLogger();
                 });
             })
             .ConfigurePlugins(a =>
             {
                 //a.UseCors("cors");//全局跨域设定

                 a.UseTcpSessionCheckClear();

                 a.Add<AuthenticationPlugin>();

                 a.UseWebApi(options =>
                 {
                     options.ConfigureConverter(converter =>
                     {
                         #region WebApi配置序列化器
                         //配置转换器

                         //converter.Clear();//可以选择性的清空现有所有格式化器

                         //添加Json格式化器,可以自定义Json的一些设置
                         converter.AddJsonSerializerFormatter(new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

                         //添加Xml格式化器
                         //converter.AddXmlSerializerFormatter();

                         //converter.Add(new MySerializerFormatter());
                         #endregion
                     });
                 });

                 a.UseSwagger(options =>
                 {
                     options.UseLaunchBrowser();
                 });//使用Swagger页面

                 //此插件是http的兜底插件，应该最后添加。作用是当所有路由不匹配时返回404.且内部也会处理Option请求。可以更好的处理来自浏览器的跨域探测。
                 a.UseDefaultHttpServicePlugin();
             }));
        await service.StartAsync();

        Console.WriteLine("以下连接用于测试webApi");
        Console.WriteLine($"使用：http://127.0.0.1:7789/ApiServer/Sum?a=10&b=20");
        Console.ReadKey();
    }

    private static async Task SimpleStart()
    {
        #region 启动WebApi服务器
        var service = new HttpService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(7789)
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<DemoApiServer>();//注册服务
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseTcpSessionCheckClear();

                a.UseWebApi();

                //此插件是http的兜底插件，应该最后添加。作用是当所有路由不匹配时返回404.且内部也会处理Option请求。可以更好的处理来自浏览器的跨域探测。
                a.UseDefaultHttpServicePlugin();
            }));
        await service.StartAsync();

        Console.WriteLine("以下连接用于测试webApi");
        Console.WriteLine($"使用：http://127.0.0.1:7789/ApiServer/Sum?a=10&b=20");
        #endregion

    }
}
