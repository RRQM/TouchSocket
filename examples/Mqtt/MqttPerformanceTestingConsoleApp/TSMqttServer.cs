// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;

namespace MqttPerformanceTestingConsoleApp;

/// <summary>
/// TouchSocket MQTT 服务器启动入口
/// </summary>
internal static class TSMqttServer
{
    /// <summary>
    /// 启动 TouchSocket MQTT 服务器
    /// </summary>
    public static async Task StartAsync()
    {
        var (host, port) = BenchmarkConfig.ReadListenConfig();

        var service = new MqttTcpService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts($"{host}:{port}")
            .SetMqttBrokerOption(o =>
            {
                o.MessageCapacity = int.MaxValue;
                o.PublishConcurrency = 5;
            })
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                a.AddMqttConnectingPlugin(async (client, e) =>
                {
                    e.ConnAckMessage.ReturnCode = MqttReasonCode.ConnectionAccepted;
                    await e.InvokeNext();
                });
            }));

        await service.StartAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[TSMqtt] 服务器已启动，监听地址: {host}:{port}");
        Console.ResetColor();
        Console.WriteLine("按 Enter 键停止服务器...");
        Console.ReadLine();

        await service.StopAsync();
        Console.WriteLine("[TSMqtt] 服务器已停止。");
    }
}
