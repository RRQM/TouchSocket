namespace TrafficCounterConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //TcpService service = new TcpService();
            //service.Received = (client, byteBlock, requestInfo) =>
            //{
            //    //从客户端收到信息
            //    string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Length);
            //    Console.WriteLine($"已从{client.Id}接收到信息：{mes}");
            //    client.Send(mes);//将收到的信息直接返回给发送方
            //};

            //service.SetupAsync(new TouchSocketConfig()//载入配置
            //    .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
            //    .SetMaxCount(1000)
            //    .SetThreadCount(10)
            //    .ConfigurePlugins(a =>
            //    {
            //        a.Add<TrafficCounterPlugin>(new object[] { false });//此处可以添加插件
            //    })
            //    .ConfigureContainer(a =>
            //    {
            //        a.AddConsoleLogger();//添加一个日志注入
            //    }))
            //    .StartAsync();//启动
            //Timer timer = new Timer((s) =>
            //{
            //    var clients = service.GetClients();
            //    foreach (var item in clients)
            //    {
            //        item.Logger.Info($"发送流量：{item.GetSendTrafficCounter()}");
            //        item.Logger.Info($"接收流量：{item.GetReceivedTrafficCounter()}");
            //    }
            //}, null, 0, 1000);

            //Console.ReadKey();
        }
    }

    //public class TrafficCounterPlugin : TcpPluginBase
    //{
    //    public bool AutoRefresh { get; }

    //    [DependencyInject(true)]
    //    public TrafficCounterPlugin(bool autoRefresh)
    //    {
    //        this.AutoRefresh = autoRefresh;
    //    }

    //    protected override void OnConnected(ITcpSession client, TouchSocketEventArgs e)
    //    {
    //        client.SetValue(TrafficCounterEx.AutoRefreshProperty, this.AutoRefresh);
    //        if (this.AutoRefresh)
    //        {
    //            client.SetValue(TrafficCounterEx.AutoRefreshTimerProperty, new Timer((s) =>
    //            {
    //                var countSend = client.GetValue<int>(TrafficCounterEx.SendTempTrafficCounterProperty);
    //                client.SetValue(TrafficCounterEx.SendTempTrafficCounterProperty, 0);
    //                client.SetValue(TrafficCounterEx.SendTrafficCounterProperty, countSend);

    //                var countRev = client.GetValue<int>(TrafficCounterEx.ReceivedTempTrafficCounterProperty);
    //                client.SetValue(TrafficCounterEx.ReceivedTempTrafficCounterProperty, 0);
    //                client.SetValue(TrafficCounterEx.ReceivedTrafficCounterProperty, countRev);
    //            }, null, 0, 1000));
    //        }

    //        base.OnConnected(client, e);
    //    }

    //    protected override void OnSending(ITcpSession client, SendingEventArgs e)
    //    {
    //        client.SetValue(TrafficCounterEx.SendTempTrafficCounterProperty,
    //            e.Length + client.GetValue<int>(TrafficCounterEx.SendTempTrafficCounterProperty));
    //        base.OnSending(client, e);
    //    }

    //    protected override void OnReceivingData(ITcpSession client, ByteBlockEventArgs e)
    //    {
    //        client.SetValue(TrafficCounterEx.ReceivedTempTrafficCounterProperty,
    //            e.ByteBlock.Length + +client.GetValue<int>(TrafficCounterEx.ReceivedTempTrafficCounterProperty));
    //        base.OnReceivingData(client, e);
    //    }
    //}

    //public static class TrafficCounterEx
    //{
    //    public static readonly DependencyProperty<int> SendTrafficCounterProperty =
    //        DependencyProperty<int>.Register("SendTrafficCounter", typeof(TrafficCounterEx), 0);

    //    public static readonly DependencyProperty<int> SendTempTrafficCounterProperty =
    //       DependencyProperty<int>.Register("SendTempTrafficCounter", typeof(TrafficCounterEx), 0);

    //    public static readonly DependencyProperty<int> ReceivedTrafficCounterProperty = DependencyProperty<int>.Register("ReceivedTrafficCounter", typeof(TrafficCounterEx), 0);

    //    public static readonly DependencyProperty<int> ReceivedTempTrafficCounterProperty = DependencyProperty<int>.Register("ReceivedTempTrafficCounter", typeof(TrafficCounterEx), 0);

    //    public static readonly DependencyProperty<bool> AutoRefreshProperty =
    //       DependencyProperty<bool>.Register("AutoRefresh", typeof(TrafficCounterEx), true);

    //    public static readonly DependencyProperty<Timer> AutoRefreshTimerProperty =
    //       DependencyProperty<Timer>.Register("AutoRefreshTimer", typeof(TrafficCounterEx), null);

    //    public static int GetSendTrafficCounter(this DependencyObject dependencyObject)
    //    {
    //        if (dependencyObject.GetValue<bool>(AutoRefreshProperty))
    //        {
    //            return dependencyObject.GetValue<int>(SendTrafficCounterProperty);
    //        }
    //        else
    //        {
    //            var count = dependencyObject.GetValue<int>(SendTempTrafficCounterProperty);
    //            dependencyObject.SetValue(SendTempTrafficCounterProperty, 0);

    //            dependencyObject.SetValue(SendTrafficCounterProperty, count);
    //            return count;
    //        }
    //    }

    //    public static int GetReceivedTrafficCounter(this DependencyObject dependencyObject)
    //    {
    //        if (dependencyObject.GetValue<bool>(AutoRefreshProperty))
    //        {
    //            return dependencyObject.GetValue<int>(ReceivedTrafficCounterProperty);
    //        }
    //        else
    //        {
    //            var count = dependencyObject.GetValue<int>(ReceivedTempTrafficCounterProperty);
    //            dependencyObject.SetValue(ReceivedTempTrafficCounterProperty, 0);

    //            dependencyObject.SetValue(ReceivedTrafficCounterProperty, count);
    //            return count;
    //        }
    //    }
    //}
}