//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RpcArgsClassLib;
using RRQMService.XUnitTest.Server;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Data;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Http;
using TouchSocket.Http.Plugins;
using TouchSocket.Http.WebSockets;
using TouchSocket.Http.WebSockets.Plugins;
using TouchSocket.Rpc;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.TouchRpc.Plugins;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Rpc.XmlRpc;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace RRQMService.XUnitTest
{
    public static class XUnitTestDemo
    {
        public static void Start()
        {
            ThreadPool.SetMinThreads(50, 50);
            CreateTcpService(7789);
            CreateUdpService(7790, 7791);
            CreateHttpService(7801);

            CreateRpcRpcStore();//7794,7797,7800,7801,7802,7803,7804,8848

            CreateTLVTcpService(7805);
        }

        private static HttpTouchRpcService m_httpService;

        private static void CreateHttpService(int port)
        {
            m_httpService = new HttpTouchRpcService();
            m_httpService.Setup(new TouchSocketConfig()
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetVerifyToken("123RPC")
                .SetThreadCount(50)
                .ConfigurePlugins(a =>
                {
                    a.Add<WebSocketServerPlugin>();
                    a.Add<MyHttpPlugin>();
                    a.Add<MyWebSocketPlugin>();
                    a.Add<MyHttpTouchRpcPlugin>();
                }))
                .Start();


        }


        private static void CreateRpcRpcStore()
        {
            RpcStore rpcService = new RpcStore(new Container());
            rpcService.OnRequestProxy = (request) =>
            {
                if (request.Query.Get("token") == "123")
                {
                    return true;
                }
                return false;
            };

            CodeGenerator.AddProxyType<RpcArgsClassLib.ProxyClass1>();
            CodeGenerator.AddProxyType<RpcArgsClassLib.ProxyClass2>(deepSearch: true);

            rpcService.AddRpcParser("m_httpService", m_httpService);
            rpcService.AddRpcParser("tcpRPCParser", CreateRRQMTcpParser(7794));

            rpcService.AddRpcParser("udpRPCParser", CreateRRQMUdpParser(7797));

            rpcService.AddRpcParser("webApiParser", m_httpService.AddPlugin<WebApiParserPlugin>());

            rpcService.AddRpcParser("xmlRpcParser", m_httpService.AddPlugin<XmlRpcParserPlugin>());

            rpcService.AddRpcParser("JsonRpcParser_Tcp", CreateTcpJsonRpcParser(7803));
            Console.WriteLine($"JsonRpcParser_Tcp解析器添加完成，端口号：{7803}");

            rpcService.AddRpcParser("JsonRpcParser_Http", m_httpService.AddPlugin<JsonRpcParserPlugin>().SetJsonRpcUrl("/jsonRpc"));

            rpcService.RegisterServer<XUnitTestController>();//注册服务
            rpcService.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();//注册服务

            rpcService.ShareProxy(new IPHost(8848));
            Console.WriteLine("RPC代理已开启分享，请通过8848端口获取。");
            Console.WriteLine();
            Console.WriteLine("以下连接用于测试webApi");
            Console.WriteLine($"使用：http://127.0.0.1:7801/XUnitTestController/Sum?a=10&b=20");
            Console.WriteLine($"使用：http://127.0.0.1:7801/XUnitTestController/Test15_ReturnArgs");
            Console.WriteLine($"使用：http://127.0.0.1:7801/XUnitTestController/Test14_ListClass01?length=10");

            //string code = RpcStore.GetProxyInfo("http://127.0.0.1:8848/proxy?proxy=all&token=123");

            var serverCellCodes = rpcService.GetProxyCodes("RpcProxy");
            File.WriteAllText("../../../XunitRpcProxy.cs", serverCellCodes);
            //string code = CodeGenerator.ConvertToCode("RRQM", proxyInfo.Codes);
            //Console.WriteLine(code);
        }

        private static IRpcParser CreateRRQMTcpParser(int port)
        {
            TcpTouchRpcService tcpRpcParser = new TcpTouchRpcService();
            TouchSocketConfig config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetVerifyToken("123RPC")
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.Add<MyTcpTouchRpcPlugin>();
                });

            //载入配置
            tcpRpcParser.Setup(config);

            //启动服务
            tcpRpcParser.Start();

            Console.WriteLine($"TCP解析器添加完成，端口号：{port}，VerifyToken=123RPC");
            return tcpRpcParser;
        }


        private static IRpcParser CreateRRQMUdpParser(int port)
        {
            UdpTouchRpc udpRpcParser = new UdpTouchRpc();

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost(port))
                .SetBufferLength(1024)
                .SetThreadCount(1);

            udpRpcParser.Setup(config);

            udpRpcParser.Start();

            Console.WriteLine($"UDP解析器添加完成，端口号：{port}");
            return udpRpcParser;
        }

        private static IRpcParser CreateTcpJsonRpcParser(int port)
        {
            TcpService service = new TcpService();
            service.Connecting += (client, e) =>
            {
                client.SetDataHandlingAdapter(new TerminatorPackageAdapter("\r\n"));
            };

            service.Setup(new TouchSocketConfig().UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(port) }))
                .Start();

            return service.AddPlugin<JsonRpcParserPlugin>();
        }

        private static void CreateTcpService(int port)
        {
            TcpService tcpService = new TcpService();

            //订阅初始化事件
            tcpService.Connecting += (arg1, arg2) =>
            {
                arg1.SetDataHandlingAdapter(new NormalDataHandlingAdapter());//设置数据处理适配器
            };

            //订阅连接事件
            tcpService.Connected += (client, e) =>
            {
                Console.WriteLine("客户端已连接到TcpService");
            };

            //订阅收到消息事件
            tcpService.Received += (client, byteBlock, requestInfo) =>
            {
                client.Send(byteBlock);
            };

            //订阅断开连接事件
            tcpService.Disconnected += (client, e) =>
            {
                Console.WriteLine("客户端已断开");
            };

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetBufferLength(1024)
                .SetThreadCount(1);

            //载入配置
            tcpService.Setup(config);

            //启动
            tcpService.Start();
            Console.WriteLine($"TcpService已启动,端口：{port}");
        }

        private static void CreateTLVTcpService(int port)
        {
            TcpService tcpService = new TcpService();

            //订阅收到消息事件
            tcpService.Received += (client, byteBlock, requestInfo) =>
            {
                if (requestInfo is TLVDataFrame frame)
                {
                    client.Send(frame);
                }

            };

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetBufferLength(1024)
                .SetThreadCount(1)
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.Add<TLVPlugin>();
                });

            //载入配置
            tcpService.Setup(config);

            //启动
            tcpService.Start();
            Console.WriteLine($"TLVTcpService已启动,端口：{port}");
        }

        private static void CreateUdpService(int bindPort, int targetPort)
        {
            UdpSession udpSession = new UdpSession();
            udpSession.Received += (endpoint, byteBlock, requestInfo) =>
            {
                lock (udpSession)
                {
                    udpSession.Send(endpoint, byteBlock);//将接收到的数据发送至默认终端
                }
            };

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost($"127.0.0.1:{bindPort}"))
                .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())
                .SetBufferLength(1024 * 1024);

            udpSession.Setup(config);//加载配置
            udpSession.Start();//启动
            Console.WriteLine($"UdpService已启动，监听端口：{bindPort}，目标端口：{targetPort}");
        }


    }

    class MyTcpTouchRpcPlugin : TouchRpcPluginBase
    {
        protected override void OnStreamTransfering(ITouchRpc client, StreamOperationEventArgs e)
        {
            e.Bucket = new MemoryStream();
            Console.WriteLine("TcpRpcParser_StreamTransfering");
            base.OnStreamTransfering(client, e);
        }

        protected override void OnStreamTransfered(ITouchRpc client, StreamStatusEventArgs e)
        {
            Console.WriteLine("TcpRpcParser_StreamTransfered");
            if (e.Bucket.Length == 1024 * 1024 * 100 && e.Metadata["1"] == "1" && e.Result.ResultCode == ResultCode.Success)
            {
                client.Send(20000);
            }

            e.Bucket.SafeDispose();
            base.OnStreamTransfered(client, e);
        }

        protected override void OnReceivedProtocolData(ITouchRpc client, ProtocolDataEventArgs e)
        {
            short protocol = e.Protocol;
            ByteBlock byteBlock = e.ByteBlock;
            Console.WriteLine($"TcpTouchRpcService收到数据，协议为：{protocol}，数据长度为：{byteBlock.Len - 2}");

            if (protocol > 0)
            {
                switch (protocol)
                {
                    case 10000:
                        {
                            if (client.TrySubscribeChannel(TouchSocketBitConverter.Default.ToInt32(byteBlock.Buffer, 2), out Channel channel))
                            {
                                Task.Run(() =>
                                {
                                    while (channel.MoveNext())
                                    {
                                        Console.WriteLine(channel.GetCurrent().Length);
                                    }
                                    Console.WriteLine($"通道接收结束，状态{channel.Status}");
                                    if (channel.Status == ChannelStatus.Completed)
                                    {
                                        client.Send(10000);
                                    }
                                });
                            }
                            break;
                        }
                    case 10001:
                        {
                            string id = byteBlock.Seek(2).ReadString();
                            Task.Run(() =>
                            {
                                try
                                {
                                    Console.WriteLine($"oldID={client.ID},newID={id}");
                                    client.ResetID(id);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"重置ID错误，{ex.Message}");
                                }
                            });

                            break;
                        }
                    case 20001:
                        {
                            Task.Run(() =>
                            {
                                using (MemoryStream stream = new MemoryStream(new byte[1024 * 1024 * 100]))
                                {
                                    StreamOperator streamOperator = new StreamOperator();
                                    Metadata metadata = new Metadata();
                                    metadata.Add("1", "1");
                                    Result result = client.SendStream(stream, streamOperator, metadata);
                                    Console.WriteLine($"服务器发送流数据结束，状态{result}");
                                    Console.WriteLine($"服务器发送流数据结束，状态{streamOperator.Result}");
                                }
                            });

                            break;
                        }
                    default:
                        client.Send(protocol, byteBlock.ToArray(2));
                        break;
                }
            }
            else
            {
                ((TcpTouchRpcSocketClient)client).Send(byteBlock.ToArray(2));
            }
            base.OnReceivedProtocolData(client, e);
        }
    }
    class MyHttpTouchRpcPlugin : TouchRpcPluginBase
    {
        protected override void OnStreamTransfering(ITouchRpc client, StreamOperationEventArgs e)
        {
            e.Bucket = new MemoryStream();
            Console.WriteLine("httpService_StreamTransfering");
            base.OnStreamTransfering(client, e);
        }

        protected override Task OnStreamTransferedAsync(ITouchRpc client, StreamStatusEventArgs e)
        {
            Console.WriteLine("TcpRpcParser_StreamTransfered");
            if (e.Bucket.Length == 1024 * 1024 * 100 && e.Metadata["1"] == "1" && e.Result.ResultCode == ResultCode.Success)
            {
                client.Send(20000);
            }

            e.Bucket.SafeDispose();
            return Task.FromResult(0);
        }

        protected override void OnReceivedProtocolData(ITouchRpc client, ProtocolDataEventArgs e)
        {
            short protocol = e.Protocol;
            ByteBlock byteBlock = e.ByteBlock;
            Console.WriteLine($"TcpTouchRpcService收到数据，协议为：{protocol}，数据长度为：{byteBlock.Len - 2}");

            if (protocol > 0)
            {
                switch (protocol)
                {
                    case 10000:
                        {
                            if (client.TrySubscribeChannel(TouchSocketBitConverter.Default.ToInt32(byteBlock.Buffer, 2), out Channel channel))
                            {
                                Task.Run(() =>
                                {
                                    channel.MaxSpeed = int.MaxValue;
                                    while (channel.MoveNext())
                                    {
                                        Console.WriteLine(channel.GetCurrent().Length);
                                    }
                                    Console.WriteLine($"通道接收结束，状态{channel.Status}");
                                    if (channel.Status == ChannelStatus.Completed)
                                    {
                                        client.Send(10000);
                                    }
                                });
                            }
                            break;
                        }
                    case 10001:
                        {
                            string id = byteBlock.Seek(2).ReadString();
                            Task.Run(() =>
                            {
                                try
                                {
                                    Console.WriteLine($"oldID={client.ID},newID={id}");
                                    client.ResetID(id);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"重置ID错误，{ex.Message}");
                                }
                            });

                            break;
                        }
                    case 20001:
                        {
                            Task.Run(() =>
                            {
                                using (MemoryStream stream = new MemoryStream(new byte[1024 * 1024 * 100]))
                                {
                                    StreamOperator streamOperator = new StreamOperator();
                                    Metadata metadata = new Metadata();
                                    metadata.Add("1", "1");
                                    Result result = client.SendStream(stream, streamOperator, metadata);
                                    Console.WriteLine($"服务器发送流数据结束，状态{result}");
                                    Console.WriteLine($"服务器发送流数据结束，状态{streamOperator.Result}");
                                }
                            });

                            break;
                        }
                    default:
                        client.Send(protocol, byteBlock.ToArray(2));
                        break;
                }
            }
            else
            {
                ((HttpTouchRpcSocketClient)client).Send(byteBlock.ToArray(2));
            }
            base.OnReceivedProtocolData(client, e);
        }
    }

    internal class MyHttpPlugin : HttpPluginBase
    {
        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (e.Context.Request.UrlEquals("/xunit"))
            {
                e.Handled = true;

                e.Context.Response
                    .FromText("OK")
                    .SetStatus()
                    .Answer();
            }
            base.OnGet(client, e);
        }
    }

    internal class MyWebSocketPlugin : WebSocketPluginBase
    {
        protected override void OnConnected(ITcpClientBase client, TouchSocketEventArgs e)
        {
            Console.WriteLine("TCP连接");
            base.OnConnected(client, e);
        }

        protected override void OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {
            Console.WriteLine("WebSocket正在连接");
            //e.IsPermitOperation = false;表示拒绝
            base.OnHandshaking(client, e);
        }

        protected override void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Text)
            {
                e.Handled = true;
                client.Logger.Info(e.DataFrame.ToText());
            }
            base.OnHandleWSDataFrame(client, e);
        }

        protected override void OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {
            Console.WriteLine("WebSocket成功连接");
            base.OnHandshaked(client, e);
        }

        protected override void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("TCP断开连接");
            base.OnDisconnected(client, e);
        }
    }
}