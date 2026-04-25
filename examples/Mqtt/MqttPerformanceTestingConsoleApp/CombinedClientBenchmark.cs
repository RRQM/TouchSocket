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

namespace MqttPerformanceTestingConsoleApp;

/// <summary>
/// 组合压测入口：依次运行 MQTTnet 和 TSMqtt 客户端，最终统一输出表格报告
/// </summary>
internal static class CombinedClientBenchmark
{
    /// <summary>
    /// 读取目标地址后，依次执行两套客户端压测并输出汇总表格
    /// </summary>
    public static async Task RunAsync()
    {
        var (host, port) = BenchmarkConfig.ReadTargetConfig();
        var startTime = DateTime.Now;
        var allResults = new List<ScenarioResult>();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n===== 开始 MQTTnet 客户端压测 (目标: {host}:{port}) =====");
        Console.ResetColor();

        var mqttNetResults = await MqttNetClientBenchmark.RunAllAsync(host, port);
        allResults.AddRange(mqttNetResults);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n===== 开始 TSMqtt 客户端压测 (目标: {host}:{port}) =====");
        Console.ResetColor();

        var tsMqttResults = await TSMqttClientBenchmark.RunAllAsync(host, port);
        allResults.AddRange(tsMqttResults);

        var endTime = DateTime.Now;

        BenchmarkTablePrinter.Print(host, port, startTime, endTime, allResults);
        BenchmarkTablePrinter.PrintComparison(mqttNetResults, tsMqttResults);
    }
}
