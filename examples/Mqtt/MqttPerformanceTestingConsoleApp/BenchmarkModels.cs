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

using Spectre.Console;

namespace MqttPerformanceTestingConsoleApp;

/// <summary>
/// 单条压测场景结果
/// </summary>
internal sealed class ScenarioResult
{
    /// <summary>
    /// 场景名称（拓扑描述，如"1发布×1订阅"）
    /// </summary>
    public string ScenarioName { get; set; } = string.Empty;

    /// <summary>
    /// 客户端类型标识（如"MQTTnet"或"TSMqtt"）
    /// </summary>
    public string ClientType { get; set; } = string.Empty;

    /// <summary>
    /// 消息质量等级标识（"QoS0"/"QoS1"/"QoS2"）
    /// </summary>
    public string QosLabel { get; set; } = string.Empty;

    /// <summary>
    /// 发布者数量
    /// </summary>
    public int PublisherCount { get; set; }

    /// <summary>
    /// 订阅者数量
    /// </summary>
    public int SubscriberCount { get; set; }

    /// <summary>
    /// 每个发布者发送的消息数
    /// </summary>
    public int MessagesPerPublisher { get; set; }

    /// <summary>
    /// 期望收到的总消息数
    /// </summary>
    public long ExpectedTotalReceived { get; set; }

    /// <summary>
    /// 实际收到的总消息数
    /// </summary>
    public long ActualTotalReceived { get; set; }

    /// <summary>
    /// 总发送消息数
    /// </summary>
    public long TotalSent { get; set; }

    /// <summary>
    /// 持续时间
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// 发送吞吐量（消息/秒）
    /// </summary>
    public double SendThroughput => Duration.TotalSeconds > 0 ? TotalSent / Duration.TotalSeconds : 0;

    /// <summary>
    /// 接收吞吐量（消息/秒）
    /// </summary>
    public double ReceiveThroughput => Duration.TotalSeconds > 0 ? ActualTotalReceived / Duration.TotalSeconds : 0;

    /// <summary>
    /// 消息丢失数（原始值，可能为负表示重复投递）
    /// </summary>
    public long LostMessages => ExpectedTotalReceived - ActualTotalReceived;

    /// <summary>
    /// 用于显示的丢失数，小于 0 时视为 0（QoS1/2 重复投递属正常）
    /// </summary>
    public long DisplayLostMessages => Math.Max(0L, LostMessages);

    /// <summary>
    /// 是否成功（实际收到不少于期望值）
    /// </summary>
    public bool IsSuccess => ActualTotalReceived >= ExpectedTotalReceived;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 读取压测配置的辅助类
/// </summary>
internal static class BenchmarkConfig
{
    /// <summary>
    /// 读取服务器监听配置（地址 + 端口），含默认值
    /// </summary>
    public static (string host, int port) ReadListenConfig()
    {
        Console.Write("请输入监听地址 [默认 127.0.0.1]: ");
        var host = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(host)) host = "127.0.0.1";

        Console.Write("请输入监听端口 [默认 7789]: ");
        var portStr = Console.ReadLine()?.Trim();
        var port = string.IsNullOrEmpty(portStr) ? 7789 : int.Parse(portStr);

        return (host, port);
    }

    /// <summary>
    /// 读取客户端目标配置（地址 + 端口），含默认值
    /// </summary>
    public static (string host, int port) ReadTargetConfig()
    {
        Console.Write("请输入目标地址 [默认 127.0.0.1]: ");
        var host = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(host)) host = "127.0.0.1";

        Console.Write("请输入目标端口 [默认 7789]: ");
        var portStr = Console.ReadLine()?.Trim();
        var port = string.IsNullOrEmpty(portStr) ? 7789 : int.Parse(portStr);

        return (host, port);
    }
}

/// <summary>
/// 将所有场景结果以 Spectre.Console 表格形式统一输出
/// </summary>
internal static class BenchmarkTablePrinter
{
    /// <summary>
    /// 打印统一对比表格
    /// </summary>
    public static void Print(
        string targetHost, int targetPort,
        DateTime startTime, DateTime endTime,
        IReadOnlyList<ScenarioResult> results)
    {
        var elapsed = (endTime - startTime).TotalSeconds;

        AnsiConsole.MarkupLine(
            $"[cyan]MQTT 客户端压测汇总报告[/]  " +
            $"目标: [yellow]{targetHost}:{targetPort}[/]  " +
            $"{startTime:yyyy-MM-dd HH:mm:ss} ~ {endTime:HH:mm:ss}  " +
            $"总耗时: [yellow]{elapsed:F1}s[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.Border = TableBorder.Ascii2;
        table.BorderStyle = Style.Plain;

        table.AddColumn(new TableColumn("场景名称").LeftAligned());
        table.AddColumn(new TableColumn("客户端").Centered());
        table.AddColumn(new TableColumn("QoS").Centered());
        table.AddColumn(new TableColumn("发布x订阅").Centered());
        table.AddColumn(new TableColumn("发送量").RightAligned());
        table.AddColumn(new TableColumn("实际收").RightAligned());
        table.AddColumn(new TableColumn("丢包").RightAligned());
        table.AddColumn(new TableColumn("耗时(s)").RightAligned());
        table.AddColumn(new TableColumn("发送/s").RightAligned());
        table.AddColumn(new TableColumn("接收/s)").RightAligned());
        table.AddColumn(new TableColumn("状态").Centered());

        var prevScene = string.Empty;
        foreach (var r in results)
        {
            if (prevScene.Length > 0 && r.ScenarioName != prevScene)
            {
                table.AddEmptyRow();
            }
            prevScene = r.ScenarioName;

            var lostStr = r.DisplayLostMessages == 0 ? "-" : r.DisplayLostMessages.ToString("N0");
            var statusMarkup = r.IsSuccess ? "[green]OK[/]" : "[red]FAIL[/]";
            var lostMarkup = r.DisplayLostMessages == 0 ? lostStr : $"[red]{lostStr}[/]";

            table.AddRow(
                Markup.Escape(r.ScenarioName),
                r.ClientType,
                r.QosLabel,
                $"{r.PublisherCount}x{r.SubscriberCount}",
                r.TotalSent.ToString("N0"),
                r.ActualTotalReceived.ToString("N0"),
                lostMarkup,
                r.Duration.TotalSeconds.ToString("F2"),
                ((long)r.SendThroughput).ToString("N0"),
                ((long)r.ReceiveThroughput).ToString("N0"),
                statusMarkup);

            if (!string.IsNullOrEmpty(r.ErrorMessage))
            {
                table.AddRow(
                    $"[red]  ERR: {Markup.Escape(r.ErrorMessage)}[/]",
                    "", "", "", "", "", "", "", "", "", "");
            }
        }

        // 汇总行
        table.AddEmptyRow();
        var totalSent = results.Sum(r => r.TotalSent);
        var totalRecv = results.Sum(r => r.ActualTotalReceived);
        var totalLost = results.Sum(r => r.DisplayLostMessages);
        var successCnt = results.Count(r => r.IsSuccess);
        var avgSend = results.Count > 0 ? results.Average(r => r.SendThroughput) : 0;
        var avgRecv = results.Count > 0 ? results.Average(r => r.ReceiveThroughput) : 0;
        var totalLostMarkup = totalLost == 0 ? "-" : $"[red]{totalLost:N0}[/]";

        table.AddRow(
            "[bold]合计 / 平均[/]",
            $"[bold]{successCnt}/{results.Count}[/]",
            "-", "-",
            $"[bold]{totalSent:N0}[/]",
            $"[bold]{totalRecv:N0}[/]",
            $"[bold]{totalLostMarkup}[/]",
            "-",
            $"[bold]{(long)avgSend:N0}[/]",
            $"[bold]{(long)avgRecv:N0}[/]",
            successCnt == results.Count ? "[green][bold]全部OK[/][/]" : "[red][bold]有失败[/][/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 打印 MQTTnet vs TSMqtt 对比表格
    /// </summary>
    public static void PrintComparison(
        IReadOnlyList<ScenarioResult> mqttNetResults,
        IReadOnlyList<ScenarioResult> tsMqttResults)
    {
        AnsiConsole.MarkupLine("[cyan]MQTTnet vs TSMqtt 对比[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.Border = TableBorder.Ascii2;
        table.BorderStyle = Style.Plain;

        table.AddColumn(new TableColumn("场景").LeftAligned());
        table.AddColumn(new TableColumn("QoS").Centered());
        table.AddColumn(new TableColumn("MQTTnet 接收/s").RightAligned());
        table.AddColumn(new TableColumn("TSMqtt 接收/s").RightAligned());
        table.AddColumn(new TableColumn("接收/s 差距").RightAligned());
        table.AddColumn(new TableColumn("MQTTnet 耗时(s)").RightAligned());
        table.AddColumn(new TableColumn("TSMqtt 耗时(s)").RightAligned());
        table.AddColumn(new TableColumn("耗时差(s)").RightAligned());

        var mnDict = mqttNetResults.ToDictionary(r => (r.ScenarioName, r.QosLabel));
        var tsDict = tsMqttResults.ToDictionary(r => (r.ScenarioName, r.QosLabel));

        var keys = mnDict.Keys.Union(tsDict.Keys)
            .OrderBy(k => k.ScenarioName)
            .ThenBy(k => k.QosLabel)
            .ToList();

        var prevScene = string.Empty;
        foreach (var key in keys)
        {
            if (prevScene.Length > 0 && key.ScenarioName != prevScene)
            {
                table.AddEmptyRow();
            }
            prevScene = key.ScenarioName;

            mnDict.TryGetValue(key, out var mn);
            tsDict.TryGetValue(key, out var ts);

            var mnRecv = mn is not null ? (long)mn.ReceiveThroughput : 0L;
            var tsRecv = ts is not null ? (long)ts.ReceiveThroughput : 0L;
            var mnDur = mn?.Duration.TotalSeconds ?? 0;
            var tsDur = ts?.Duration.TotalSeconds ?? 0;

            var recvDiff = tsRecv - mnRecv;
            var durDiff = tsDur - mnDur;

            var recvDiffMarkup = recvDiff == 0 ? "0"
                : recvDiff > 0 ? $"[green]+{recvDiff:N0}[/]"
                : $"[red]{recvDiff:N0}[/]";

            var durDiffMarkup = durDiff == 0 ? "0"
                : durDiff < 0 ? $"[green]{durDiff:F2}[/]"
                : $"[red]+{durDiff:F2}[/]";

            table.AddRow(
                Markup.Escape(key.ScenarioName),
                key.QosLabel,
                mnRecv.ToString("N0"),
                tsRecv.ToString("N0"),
                recvDiffMarkup,
                mnDur.ToString("F2"),
                tsDur.ToString("F2"),
                durDiffMarkup);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]接收/s 差距 = TSMqtt - MQTTnet，正数表示 TSMqtt 更快；耗时差 = TSMqtt - MQTTnet，负数表示 TSMqtt 更快[/]");
        AnsiConsole.WriteLine();
    }
}
