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

using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeClientConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var client = await CreateClient();

            while (true)
            {
                await client.SendAsync(Console.ReadLine());
            }
        }

        private static async Task<NamedPipeClient> CreateClient()
        {
            var client = new NamedPipeClient();

            client.Received = (client, e) =>
            {
                //从服务器收到信息
                var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
                client.Logger.Info($"客户端接收到信息：{mes}");

                return Task.CompletedTask;
            };

            //载入配置
            await client.SetupAsync(new TouchSocketConfig()
                 .SetPipeServer(".")//一般本机管道时，可以不用此配置
                 .SetPipeName("touchsocketpipe")//管道名称
                 .ConfigurePlugins(a =>
                 {
                     a.UseNamedPipeReconnection();
                 })
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个日志注入
                 }));
            await client.ConnectAsync();
            client.Logger.Info("客户端成功连接");
            return client;
        }
    }
}