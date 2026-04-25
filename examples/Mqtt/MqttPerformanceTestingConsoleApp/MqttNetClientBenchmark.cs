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
using MQTTnet.Protocol;
using System.Diagnostics;

namespace MqttPerformanceTestingConsoleApp;

/// <summary>
/// 使用 MQTTnet 客户端进行压测的场景集合
/// </summary>
internal static class MqttNetClientBenchmark
{
    private const string TopicBase = "benchmark/mqttnet";

    // QoS 级别定义（消息数来自 BenchmarkParams）
    private static readonly (MqttQualityOfServiceLevel Qos, string Label, int[] MsgCounts)[] s_qosConfigs =
    [
        (MqttQualityOfServiceLevel.AtMostOnce,  "QoS0", BenchmarkParams.Qos0MsgCounts),
        (MqttQualityOfServiceLevel.AtLeastOnce, "QoS1", BenchmarkParams.Qos1MsgCounts),
        (MqttQualityOfServiceLevel.ExactlyOnce, "QoS2", BenchmarkParams.Qos2MsgCounts)
    ];

    // 拓扑配置（来自 BenchmarkParams）
    private static readonly (int Pub, int Sub, string Name)[] s_topoConfigs = BenchmarkParams.TopoConfigs;

    /// <summary>
    /// 运行所有场景（3拓扑 × 3 QoS = 9条），返回结果列表
    /// </summary>
    public static async Task<List<ScenarioResult>> RunAllAsync(string host, int port)
    {
        var results = new List<ScenarioResult>();

        foreach (var topo in s_topoConfigs)
        {
            foreach (var qosCfg in s_qosConfigs)
            {
                var msgCount = qosCfg.MsgCounts[Array.IndexOf(s_topoConfigs, topo)];
                var result = await RunScenarioAsync(host, port, topo.Name, topo.Pub, topo.Sub, msgCount, qosCfg.Qos, qosCfg.Label);
                results.Add(result);
            }
        }

        return results;
    }

    private static async Task<ScenarioResult> RunScenarioAsync(
        string host, int port,
        string scenarioName,
        int publisherCount, int subscriberCount, int messagesPerPublisher,
        MqttQualityOfServiceLevel qos, string qosLabel)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"  [MQTTnet] {scenarioName} {qosLabel} ...");
        Console.ResetColor();

        var result = new ScenarioResult
        {
            ScenarioName = scenarioName,
            ClientType = "MQTTnet",
            QosLabel = qosLabel,
            PublisherCount = publisherCount,
            SubscriberCount = subscriberCount,
            MessagesPerPublisher = messagesPerPublisher,
            TotalSent = (long)publisherCount * messagesPerPublisher,
            ExpectedTotalReceived = (long)publisherCount * messagesPerPublisher * subscriberCount
        };

        var receivedCount = 0L;
        var subscriberClients = new List<IMqttClient>();
        var publisherClients = new List<IMqttClient>();

        try
        {
            var factory = new MqttClientFactory();
            var payload = new byte[BenchmarkParams.PayloadSize];
            Random.Shared.NextBytes(payload);
            var topic = $"{TopicBase}/{qosLabel}/{Guid.NewGuid():N}";

            for (var i = 0; i < subscriberCount; i++)
            {
                var sub = factory.CreateMqttClient();
                sub.ApplicationMessageReceivedAsync += _ =>
                {
                    Interlocked.Increment(ref receivedCount);
                    return Task.CompletedTask;
                };

                var subOpts = new MqttClientOptionsBuilder()
                    .WithTcpServer(host, port)
                    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
                    .WithClientId($"mn-sub-{i}-{Guid.NewGuid():N}")
                    .WithCleanSession()
                    .Build();

                await sub.ConnectAsync(subOpts);
                await sub.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(qos))
                    .Build());
                subscriberClients.Add(sub);
            }

            for (var i = 0; i < publisherCount; i++)
            {
                var pub = factory.CreateMqttClient();
                var pubOpts = new MqttClientOptionsBuilder()
                    .WithTcpServer(host, port)
                    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
                    .WithClientId($"mn-pub-{i}-{Guid.NewGuid():N}")
                    .WithCleanSession()
                    .Build();

                await pub.ConnectAsync(pubOpts);
                publisherClients.Add(pub);
            }

            await Task.Delay(BenchmarkParams.ConnectDelayMs);

            var sw = Stopwatch.StartNew();

            var publishTasks = publisherClients.Select(pub => Task.Run(async () =>
            {
                for (var m = 0; m < messagesPerPublisher; m++)
                {
                    var msg = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(payload)
                        .WithQualityOfServiceLevel(qos)
                        .Build();
                    await pub.PublishAsync(msg);
                }
            })).ToArray();

            await Task.WhenAll(publishTasks);

            var deadline = DateTime.UtcNow.AddSeconds(BenchmarkParams.ReceiveTimeoutSeconds);

            var spinWait = new SpinWait();
            while (Volatile.Read(ref receivedCount) < result.ExpectedTotalReceived && DateTime.UtcNow < deadline)
            {
                spinWait.SpinOnce();
            }

            sw.Stop();
            result.ActualTotalReceived = Volatile.Read(ref receivedCount);
            result.Duration = sw.Elapsed;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.ActualTotalReceived = Volatile.Read(ref receivedCount);
            result.Duration = TimeSpan.FromSeconds(1);
        }
        finally
        {
            foreach (var c in publisherClients)
            {
                try { await c.DisconnectAsync(); } catch { }
                c.Dispose();
            }
            foreach (var c in subscriberClients)
            {
                try { await c.DisconnectAsync(); } catch { }
                c.Dispose();
            }
        }

        var status = result.IsSuccess ? "OK" : $"丢包{result.LostMessages}";
        Console.WriteLine($"    >> {result.ActualTotalReceived}/{result.ExpectedTotalReceived}  发:{result.SendThroughput:N0}/s  收:{result.ReceiveThroughput:N0}/s  [{status}]");

        return result;
    }
}
