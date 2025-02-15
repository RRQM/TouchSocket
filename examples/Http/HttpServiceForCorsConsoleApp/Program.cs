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

using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace HttpServiceForCorsConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        var service = new HttpService();
        service.SetupAsync(new TouchSocketConfig()//加载配置
            .SetListenIPHosts(7789)
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();

                //添加跨域服务
                a.AddCors(corsOption =>
                {
                    //添加跨域策略，后续使用policyName即可应用跨域策略。
                    corsOption.Add("cors", corsBuilder =>
                    {
                        corsBuilder.AllowAnyMethod()
                            .AllowAnyOrigin();
                    });
                });
            })
            .ConfigurePlugins(a =>
            {
                //应用名称为cors的跨域策略。
                a.UseCors("cors");

                //default插件应该最后添加，其作用是
                //1、为找不到的路由返回404
                //2、处理header为Option的探视跨域请求。
                a.UseDefaultHttpServicePlugin();
            }));
        service.StartAsync();

        Console.WriteLine("Http服务器已启动");
        Console.ReadKey();
    }
}