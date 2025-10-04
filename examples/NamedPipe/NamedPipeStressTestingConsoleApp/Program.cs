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
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeStressTestingConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("请输入管道名称");
            var name = Console.ReadLine();

            var service = await CreateService(name);
            var client = await CreateClient(name);

            var buffer = new byte[1024 * 1024];
            while (true)
            {
                await client.SendAsync(buffer);
            }
        }

        private static async Task<NamedPipeClient> CreateClient(string name)
        {
            var client = new NamedPipeClient();
            //载入配置
            await client.SetupAsync(new TouchSocketConfig()
                 .SetPipeServerName(".")//一般本机管道时，可以不用此配置
                 .SetPipeName(name)//管道名称
                 .ConfigurePlugins(a =>
                 {
                 })
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个日志注入
                 }));
            await client.ConnectAsync();
            client.Logger.Info("客户端成功连接");
            return client;
        }

        private static async Task<NamedPipeService> CreateService(string name)
        {
            var service = new NamedPipeService();
            service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
            service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
            service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接\

            var counter = new ValueCounter()
            {
                OnPeriod = (l) => Console.WriteLine((l / (1048576.0)).ToString("0.00")),
                Period = TimeSpan.FromSeconds(1)
            };
            service.Received = (client, e) =>
            {
                counter.Increment(e.Memory.Length);
                return EasyTask.CompletedTask;
            };
            await service.SetupAsync(new TouchSocketConfig()//载入配置
                  .SetPipeName(name)//设置命名管道名称
                  .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                  {
                      a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                  })
                  .ConfigurePlugins(a =>
                  {
                      //a.Add();//此处可以添加插件
                  }));
            await service.StartAsync();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }
}