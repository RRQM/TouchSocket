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
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThrottlingConsoleApp;

internal class Program
{
    /// <summary>
    /// 限制单个客户端的访问流量
    /// 博客连接<see href="https://blog.csdn.net/qq_40374647/article/details/125496769"/>
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        var service = new TcpService();
        service.Received = (client, e) =>
        {
            //从客户端收到信息
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
            return EasyTask.CompletedTask;
        };

        service.SetupAsync(new TouchSocketConfig()//载入配置
            .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                a.Add<MyThrottlingPlugin>();
            }));
        service.StartAsync();//启动
        service.Logger.Info("服务器已启动");
        Console.ReadLine();
    }
}

/// <summary>
/// 一个流量计数器扩展。
/// </summary>
internal static class DependencyExtensions
{
    public static readonly DependencyProperty<FlowGate> FlowGateProperty =
        new("FlowGate", null);

    public static void InitFlowGate(this IDependencyObject dependencyObject, int max)
    {
        dependencyObject.SetValue(FlowGateProperty, new FlowGate() { Maximum = max });
    }

    public static FlowGate GetFlowGate(this IDependencyObject dependencyObject)
    {
        return dependencyObject.GetValue(FlowGateProperty);
    }
}

public class MyThrottlingPlugin : PluginBase, ITcpConnectedPlugin, ITcpReceivingPlugin
{
    private readonly int m_max;

    [DependencyInject]
    public MyThrottlingPlugin(int max = 10)
    {
        this.m_max = max;
    }

    public Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        client.InitFlowGate(this.m_max);//初始化流量计数器。
        return e.InvokeNext();
    }

    public async Task OnTcpReceiving(ITcpSession client, ByteBlockEventArgs e)
    {
        await client.GetFlowGate().AddCheckWaitAsync(e.Memory.Length);
        await e.InvokeNext();
    }
}