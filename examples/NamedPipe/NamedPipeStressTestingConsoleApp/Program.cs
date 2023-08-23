﻿using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeStressTestingConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = CreateService();
            var client = CreateClient();

            byte[] buffer = new byte[1024*1024];
            while (true) 
            {
                client.Send(buffer);
            }
        }

        private static NamedPipeClient CreateClient()
        {
            var client = new NamedPipeClient();
            //载入配置
            client.Setup(new TouchSocketConfig()
                .SetPipeServer(".")//一般本机管道时，可以不用此配置
                .SetPipeName("TouchSocketPipe")//管道名称
                .ConfigurePlugins(a =>
                {

                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }))
                .Connect();
            client.Logger.Info("客户端成功连接");
            return client;
        }

        private static NamedPipeService CreateService()
        {
            var service = new NamedPipeService();
            service.Connecting = (client, e) => { };//有客户端正在连接
            service.Connected = (client, e) => { };//有客户端成功连接
            service.Disconnected = (client, e) => { };//有客户端断开连接\

            long count = 0;
            DateTime dateTime = DateTime.Now;
            service.Received = (client, byteBlock, request) =>
            {
                if (DateTime.Now-dateTime>TimeSpan.FromSeconds(1))
                {
                    Console.WriteLine((count/(1048576.0)).ToString("0.00"));
                    count = 0;
                    dateTime = DateTime.Now;
                }
                count += byteBlock.Len;
            };
            service.Setup(new TouchSocketConfig()//载入配置
                .SetPipeName("TouchSocketPipe")//设置命名管道名称
                .SetBufferLength(1024*1024)
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                }))
                .Start();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }
}