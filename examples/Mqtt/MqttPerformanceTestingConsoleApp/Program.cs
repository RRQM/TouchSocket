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

using MqttPerformanceTestingConsoleApp;
using TouchSocket.Core;

var consoleAction = new ConsoleAction("exit");

consoleAction.Add("1", "[服务器] 启动 MQTTnet 服务器", MqttNetServer.StartAsync);
consoleAction.Add("2", "[服务器] 启动 TSMqtt（TouchSocket MQTT）服务器", TSMqttServer.StartAsync);
consoleAction.Add("3", "[客户端] 双客户端联合压测（MQTTnet + TSMqtt，QoS0/1/2 x 3拓扑，统一表格报告）", CombinedClientBenchmark.RunAsync);

Console.WriteLine("说明：每个进程只应选择一个功能（服务器或客户端），请勿在同一进程中同时运行服务器和客户端。");
Console.WriteLine("      先在一个终端启动服务器（1 或 2），再在另一个终端执行客户端压测（3）。\n");
consoleAction.ShowAll();
await consoleAction.RunCommandLineAsync();

/// <summary>
/// 压测参数配置，修改此处即可调整所有场景的测试规模
/// </summary>
internal static class BenchmarkParams
{
    /// <summary>
    /// 每条消息的载荷大小（字节）
    /// </summary>
    public const int PayloadSize = 64;

    /// <summary>
    /// 订阅者全部连接后等待稳定的延迟（毫秒）
    /// </summary>
    public const int ConnectDelayMs = 1000;

    /// <summary>
    /// 等待消息接收完成的最长超时时间（秒），超时后以实际收到数量计算结果
    /// </summary>
    public const int ReceiveTimeoutSeconds = 60;

    /// <summary>
    /// QoS0 场景每发布者发送消息数，下标依次对应拓扑：1x1 / 1x50 / 50x50
    /// </summary>
    public static readonly int[] Qos0MsgCounts = [50000, 10000, 1000];

    /// <summary>
    /// QoS1 场景每发布者发送消息数，下标依次对应拓扑：1x1 / 1x50 / 50x50
    /// </summary>
    public static readonly int[] Qos1MsgCounts = [20000, 5000, 500];

    /// <summary>
    /// QoS2 场景每发布者发送消息数，下标依次对应拓扑：1x1 / 1x50 / 50x50
    /// </summary>
    public static readonly int[] Qos2MsgCounts = [5000, 1000, 100];

    /// <summary>
    /// 拓扑配置：(发布者数, 订阅者数, 场景名)
    /// </summary>
    public static readonly (int Pub, int Sub, string Name)[] TopoConfigs =
    [
        (1,  1,  "1发布x1订阅"),
        (1,  50, "1发布x50订阅"),
        (50, 50, "50发布x50订阅"),
    ];
}
