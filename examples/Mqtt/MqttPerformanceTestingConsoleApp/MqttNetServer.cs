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

using MQTTnet;
using MQTTnet.Server;

namespace MqttPerformanceTestingConsoleApp;

/// <summary>
/// MQTTnet 服务器启动入口
/// </summary>
internal static class MqttNetServer
{
    /// <summary>
    /// 启动 MQTTnet 服务器
    /// </summary>
    public static async Task StartAsync()
    {
        var (host, port) = BenchmarkConfig.ReadListenConfig();

        var serverOptions = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithMaxPendingMessagesPerClient(int.MaxValue)
            .WithDefaultEndpointPort(port)
            .Build();

        var factory = new MqttServerFactory();
        var server = factory.CreateMqttServer(serverOptions);

        server.ValidatingConnectionAsync += e =>
        {
            e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
            return Task.CompletedTask;
        };

        await server.StartAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[MQTTnet] 服务器已启动，监听地址: {host}:{port}");
        Console.ResetColor();
        Console.WriteLine("按 Enter 键停止服务器...");
        Console.ReadLine();

        await server.StopAsync();
        Console.WriteLine("[MQTTnet] 服务器已停止。");
    }
}
