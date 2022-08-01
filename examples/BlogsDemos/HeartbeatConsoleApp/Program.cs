﻿using System;
using System.Text;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.IO;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace HeartbeatConsoleApp
{
    internal class Program
    {
        private static TcpService service = new TcpService();
        private static TcpClient tcpClient = new TcpClient();

        /// <summary>
        /// 示例心跳。
        /// 博客地址<see href="https://blog.csdn.net/qq_40374647/article/details/125598921"/>
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            ConsoleAction consoleAction = new ConsoleAction();

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .SetMaxCount(10000)
                .UsePlugin()
                .SetThreadCount(10))
                .Start();//启动

            service.AddPlugin<HeartbeatAndReceivePlugin>();
            service.Logger.Message("服务器成功启动");

            tcpClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .UsePlugin()
                .SetBufferLength(1024 * 10));

            tcpClient.AddPlugin<HeartbeatAndReceivePlugin>();

            tcpClient.Connect();
            tcpClient.Logger.Message("客户端成功连接");

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

        protected override void PreviewSend(IRequestInfo requestInfo, bool isAsync)
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
            byteBlock.Write(Data);
        }

        public byte[] PackageAsBytes()
        {
            using ByteBlock byteBlock = new ByteBlock();
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
        public static readonly DependencyProperty HeartbeatTimerProperty =
            DependencyProperty.Register("HeartbeatTimer", typeof(Timer), typeof(DependencyExtensions), null);

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

    internal class HeartbeatAndReceivePlugin : TcpPluginBase
    {
        private readonly int m_timeTick;

        [DependencyInject(1000 * 5)]
        public HeartbeatAndReceivePlugin(int timeTick)
        {
            this.m_timeTick = timeTick;
        }

        protected override void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            client.SetDataHandlingAdapter(new MyFixedHeaderDataHandlingAdapter());//设置适配器。
            base.OnConnecting(client, e);
        }

        protected override void OnConnected(ITcpClientBase client, TouchSocketEventArgs e)
        {
            if (client is ISocketClient)
            {
                return;//此处可判断，如果为服务器，则不用使用心跳。
            }

            if (client.GetValue<Timer>(DependencyExtensions.HeartbeatTimerProperty) is Timer timer)
            {
                timer.Dispose();
            }

            client.SetValue(DependencyExtensions.HeartbeatTimerProperty, new Timer((o) =>
             {
                 client.Ping();
             }, null, 0, m_timeTick));

            base.OnConnected(client, e);
        }

        protected override void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            base.OnDisconnected(client, e);
            if (client.GetValue<Timer>(DependencyExtensions.HeartbeatTimerProperty) is Timer timer)
            {
                timer.Dispose();
                client.SetValue(DependencyExtensions.HeartbeatTimerProperty, null);
            }
        }

        protected override void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is MyRequestInfo myRequest)
            {
                client.Logger.Message(myRequest.ToString());
                if (myRequest.DataType == DataType.Ping)
                {
                    client.Pong();
                }
            }
            base.OnReceivedData(client, e);
        }
    }
}