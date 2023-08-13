using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace HeartbeatConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// 示例心跳。
        /// 博客地址<see href="https://blog.csdn.net/qq_40374647/article/details/125598921"/>
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var consoleAction = new ConsoleAction();

            //服务器
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置
                    .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                    .SetTcpDataHandlingAdapter(() => new MyFixedHeaderDataHandlingAdapter())
                    .ConfigureContainer(a =>
                    {
                        a.AddConsoleLogger();
                    })
                    .ConfigurePlugins(a =>
                    {
                        a.Add<HeartbeatAndReceivePlugin>();
                    }))
                    .Start();//启动
            service.Logger.Info("服务器成功启动");

            //客户端
            var tcpClient = new TcpClient();
            tcpClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .SetTcpDataHandlingAdapter(() => new MyFixedHeaderDataHandlingAdapter())
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<HeartbeatAndReceivePlugin>();
                }));
            tcpClient.Connect();
            tcpClient.Logger.Info("客户端成功连接");

            consoleAction.OnException += ConsoleAction_OnException;
            consoleAction.Add("1", "发送心跳", () =>
              {
                  tcpClient.Ping();
              });
            consoleAction.Add("2", "发送数据", () =>
              {
                  tcpClient.Send(new MyRequestInfo()
                  {
                      DataType = DataType.Data,
                      Data = Encoding.UTF8.GetBytes(Console.ReadLine())
                  }
                  .PackageAsBytes());
              });
            consoleAction.ShowAll();
            while (true)
            {
                consoleAction.Run(Console.ReadLine());
            }
        }

        private static void ConsoleAction_OnException(Exception obj)
        {
            Console.WriteLine(obj);
        }
    }

    #region 数据格式解析

    internal class MyFixedHeaderDataHandlingAdapter : CustomFixedHeaderDataHandlingAdapter<MyRequestInfo>
    {
        public override int HeaderLength => 3;

        public override bool CanSendRequestInfo => false;

        protected override MyRequestInfo GetInstance()
        {
            return new MyRequestInfo();
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new NotImplementedException();
        }
    }

    internal class MyRequestInfo : IFixedHeaderRequestInfo
    {
        public DataType DataType { get; set; }
        public byte[] Data { get; set; }

        public int BodyLength { get; private set; }

        public bool OnParsingBody(byte[] body)
        {
            if (body.Length == this.BodyLength)
            {
                this.Data = body;
                return true;
            }
            return false;
        }

        public bool OnParsingHeader(byte[] header)
        {
            if (header.Length == 3)
            {
                this.BodyLength = TouchSocketBitConverter.Default.ToUInt16(header, 0) - 1;
                this.DataType = (DataType)header[2];
                return true;
            }
            return false;
        }

        public void Package(ByteBlock byteBlock)
        {
            byteBlock.Write((ushort)((this.Data == null ? 0 : this.Data.Length) + 1));
            byteBlock.Write((byte)this.DataType);
            if (this.Data != null)
            {
                byteBlock.Write(this.Data);
            }
        }

        public byte[] PackageAsBytes()
        {
            using var byteBlock = new ByteBlock();
            this.Package(byteBlock);
            return byteBlock.ToArray();
        }

        public override string ToString()
        {
            return $"数据类型={this.DataType}，数据={(this.Data == null ? "null" : Encoding.UTF8.GetString(this.Data))}";
        }
    }

    internal enum DataType : byte
    {
        Ping,
        Pong,
        Data
    }

    #endregion 数据格式解析

    /// <summary>
    /// 一个心跳计数器扩展。
    /// </summary>
    internal static class DependencyExtensions
    {
        public static readonly DependencyProperty<Timer> HeartbeatTimerProperty =
            DependencyProperty<Timer>.Register("HeartbeatTimer", null);

        public static bool Ping<TClient>(this TClient client) where TClient : ITcpClientBase
        {
            try
            {
                client.Send(new MyRequestInfo() { DataType = DataType.Ping }.PackageAsBytes());
                return true;
            }
            catch (Exception ex)
            {
                client.Logger.Exception(ex);
            }

            return false;
        }

        public static bool Pong<TClient>(this TClient client) where TClient : ITcpClientBase
        {
            try
            {
                client.Send(new MyRequestInfo() { DataType = DataType.Pong }.PackageAsBytes());
                return true;
            }
            catch (Exception ex)
            {
                client.Logger.Exception(ex);
            }

            return false;
        }
    }

    internal class HeartbeatAndReceivePlugin : PluginBase, ITcpConnectedPlugin<ITcpClientBase>, ITcpDisconnectedPlugin<ITcpClientBase>, ITcpReceivedPlugin<ITcpClientBase>
    {
        private readonly int m_timeTick;
        private readonly ILog logger;

        [DependencyInject(1000 * 5)]
        public HeartbeatAndReceivePlugin(int timeTick, ILog logger)
        {
            this.m_timeTick = timeTick;
            this.logger = logger;
        }


        async Task ITcpConnectedPlugin<ITcpClientBase>.OnTcpConnected(ITcpClientBase client, ConnectedEventArgs e)
        {
            if (client is ISocketClient)
            {
                return;//此处可判断，如果为服务器，则不用使用心跳。
            }

            if (client.GetValue(DependencyExtensions.HeartbeatTimerProperty) is Timer timer)
            {
                timer.Dispose();
            }

            client.SetValue(DependencyExtensions.HeartbeatTimerProperty, new Timer((o) =>
            {
                client.Ping();
            }, null, 0, this.m_timeTick));
            await e.InvokeNext();
        }

        async Task ITcpDisconnectedPlugin<ITcpClientBase>.OnTcpDisconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            if (client.GetValue(DependencyExtensions.HeartbeatTimerProperty) is Timer timer)
            {
                timer.Dispose();
                client.SetValue(DependencyExtensions.HeartbeatTimerProperty, null);
            }

            await e.InvokeNext();
        }

        async Task ITcpReceivedPlugin<ITcpClientBase>.OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is MyRequestInfo myRequest)
            {
                this.logger.Info(myRequest.ToString());
                if (myRequest.DataType == DataType.Ping)
                {
                    client.Pong();
                }
            }
            await e.InvokeNext();
        }
    }
}