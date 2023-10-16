//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RpcArgsClassLib;
using TouchSocket.Core;
using TouchSocket.Core.AspNetCore;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.Redis;
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Dmtp.RemoteStream;
using TouchSocket.Dmtp.RouterPackage;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;
using TouchSocket.NamedPipe;
using XUnitTestConsoleApp.Server;
using TouchSocket.Http.WebSockets;

namespace XUnitTestConsoleApp
{
    public static class XUnitTestDemo
    {
        private static HttpDmtpService m_httpService;

        private static IContainer GetContainer()
        {
            var container = new AspNetCoreContainer(new ServiceCollection());
            container.AddConsoleLogger();
            return container;
        }

        public static void Start()
        {
            var ser1 = CreateNamedPipeService("pipe7789");
            CreateNamedPipeDmtp("pipe7790");

            CreateTcpService(7789);
            //CreateUdpService(7790, 7791);
            CreateHttpService(7801);

            CreateTcpDmtp(7794);
            CreateUdpRpcParser(7797);
            CreateTcpJsonRpcParser(7803);

            CreateTLVTcpService(7805);


            CodeGenerator.AddProxyType<ProxyClass1>();
            CodeGenerator.AddProxyType<ProxyClass2>(deepSearch: true);

            var serverCellCodes = CodeGenerator.GetProxyCodes("RpcProxy",
                new Type[] { typeof(XUnitTestController), typeof(IOtherAssemblyServer) }, new Type[]
                {typeof(XunitDmtpRpcAttribute),typeof(XunitJsonRpcAttribute),typeof(XunitWebApiAttribute),typeof(XunitXmlRpcAttribute) });
            File.WriteAllText("../../../XunitRpcProxy.cs", serverCellCodes);

            {
                var builder = WebApplication.CreateBuilder();

                builder.Services.AddWebSocketDmtpService(() =>
                {
                    return new TouchSocketConfig()
                        .SetVerifyToken("123RPC")
                        .UseAspNetCoreContainer(builder.Services)
                        .SetCacheTimeoutEnable(false)
                        .ConfigureContainer(a =>
                        {
                            a.AddDmtpRouteService();
                        })
                        .ConfigurePlugins(a =>
                        {
                            a.UseDmtpRpc()
                            .ConfigureRpcStore(store =>
                            {
                                store.RegisterServer<XUnitTestController>();
                                store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();

                                string core = store.GetProxyCodes("ProxyCore", new Type[] { typeof(DmtpRpcAttribute) });
                            });
                            a.UseDmtpFileTransfer();
                            a.UseDmtpRemoteStream();
                            a.UseDmtpRedis();
                            a.UseDmtpRemoteAccess();
                            a.UseDmtpRouterPackage();

                            a.Add<MyDmtpPlugin>();
                        });
                });

                builder.Services.AddHttpMiddlewareDmtpService(() =>
                {
                    return new TouchSocketConfig()
                            .SetVerifyToken("123RPC")
                            .UseAspNetCoreContainer(builder.Services)
                            .SetCacheTimeoutEnable(false)
                            .ConfigureContainer(a =>
                            {
                                a.AddDmtpRouteService();
                            })
                            .ConfigurePlugins(a =>
                            {
                                a.UseDmtpRpc()
                                .ConfigureRpcStore(a =>
                                {
                                    a.RegisterServer<XUnitTestController>();
                                    a.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                                });
                                a.UseDmtpFileTransfer();
                                a.UseDmtpRemoteStream();
                                a.UseDmtpRedis();
                                a.UseDmtpRemoteAccess();
                                a.UseDmtpRouterPackage();

                                a.Add<MyDmtpPlugin>();
                            });
                });

                var app = builder.Build();

                app.UseWebSockets();
                app.UseWebSocketDmtp();

                app.UseHttpDmtp();

                app.RunAsync("http://127.0.0.1:7806");
            }



            while (true)
            {
                var pool = BytePool.Default;
                Console.WriteLine(pool.GetPoolSize());
                Console.ReadKey();
            }
        }

        private static void CreateHttpService(int port)
        {
            m_httpService = new HttpDmtpService();
            m_httpService.Setup(new TouchSocketConfig()
                 .SetContainer(GetContainer())
                .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetVerifyToken("123RPC")
                .ConfigureContainer(a =>
                {
                    a.AddDmtpRouteService();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseWebSocket()
                    .SetVerifyConnection((client, context) =>
                    {
                        if (context.Request.UrlEquals("/ws"))
                        {
                            return true;
                        }
                        if (context.Request.UrlEquals("/wsjsonrpc"))
                        {
                            return true;
                        }

                        if (context.Request.UrlEquals("/wsread"))
                        {
                            return true;
                        }
                        return false;
                    });
                    a.Add<MyWebSocketPlugin>();

                    a.UseWebApi()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });

                    a.UseWebSocketJsonRpc()
                    .SetAllowJsonRpc((client, context) =>
                    {
                        if (context.Request.UrlEquals("/wsjsonrpc"))
                        {
                            return Task.FromResult(true);
                        }
                        return Task.FromResult(false);
                    })
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });

                    a.UseHttpJsonRpc()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    })
                    .SetJsonRpcUrl("/jsonRpc");

                    a.UseXmlRpc()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    })
                    .SetXmlRpcUrl("/xmlrpc");

                    a.UseDmtpRpc()
                     .ConfigureRpcStore(store =>
                     {
                         store.RegisterServer<XUnitTestController>();
                         store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                     });
                    a.UseDmtpFileTransfer();
                    a.UseDmtpRemoteStream();
                    a.UseDmtpRedis();
                    a.UseDmtpRemoteAccess();
                    a.UseDmtpRouterPackage();

                    a.Add<MyDmtpPlugin>();
                    a.Add<MyHttpPlugin>();
                }))
                .Start();
            Console.WriteLine($"HttpService已启动,端口：{port}");
        }

        private static void CreateTcpJsonRpcParser(int port)
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()
                 .SetContainer(GetContainer())
                 .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .SetListenIPHosts(port)
                .ConfigurePlugins(a =>
                {
                    a.UseTcpJsonRpc()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });
                }))
                .Start();
        }

        private static NamedPipeService CreateNamedPipeService(string name)
        {
            var service = new NamedPipeService();
            var config = new TouchSocketConfig();
            config.SetPipeName(name)
                .SetContainer(GetContainer())
                .ConfigurePlugins(a =>
                {
                    a.Add<MyNamedPipePlugin>();
                });

            //载入配置
            service.Setup(config);

            //启动
            service.Start();
            Console.WriteLine($"NamedPipeService已启动,名称：{name}");
            return service;
        }

        private static void CreateTcpService(int port)
        {
            var service = new TcpService();

            //订阅收到消息事件
            service.Received = async (client, e) =>
            {
                await client.SendAsync(e.ByteBlock);
            };

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(port)
                .SetContainer(GetContainer())
                .ConfigurePlugins(a =>
                {
                    a.UseCheckClear();
                    a.Add<MyTcpPlugin>();
                });

            //载入配置
            service.Setup(config);

            //启动
            service.Start();
            Console.WriteLine($"TcpService已启动,端口：{port}");
        }

        private static void CreateTcpDmtp(int port)
        {
            var tcpDmtpService = new TcpDmtpService();
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(port)
                .SetContainer(GetContainer())
                .SetVerifyToken("123RPC")
                .SetCacheTimeoutEnable(false)
                .ConfigureContainer(a =>
                {
                    a.AddDmtpRouteService();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc()
                    .ConfigureRpcStore(a =>
                    {
                        a.RegisterServer<XUnitTestController>();
                        a.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });
                    a.UseDmtpFileTransfer();
                    a.UseDmtpRemoteStream();
                    a.UseDmtpRedis();
                    a.UseDmtpRemoteAccess();
                    a.UseDmtpRouterPackage();

                    a.Add<MyDmtpPlugin>();
                });

            //载入配置
            tcpDmtpService.Setup(config);

            //启动服务
            tcpDmtpService.Start();

            Console.WriteLine($"{tcpDmtpService.GetType()}已启动，端口号：{port}，VerifyToken={tcpDmtpService.VerifyToken}");
        }

        private static void CreateNamedPipeDmtp(string name)
        {
            var service = new NamedPipeDmtpService();
            var config = new TouchSocketConfig();
            config.SetPipeName(name)
                .SetContainer(GetContainer())
                .SetVerifyToken("123RPC")
                .SetCacheTimeoutEnable(false)
                .ConfigureContainer(a =>
                {
                    a.AddDmtpRouteService();
                })
                .ConfigurePlugins(a =>
                {
                    //a.UseAutoBufferLength();

                    a.UseDmtpRpc()
                    .ConfigureRpcStore(a =>
                    {
                        a.RegisterServer<XUnitTestController>();
                        a.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });
                    a.UseDmtpFileTransfer();
                    a.UseDmtpRemoteStream();
                    a.UseDmtpRedis();
                    a.UseDmtpRemoteAccess();
                    a.UseDmtpRouterPackage();

                    a.Add<MyDmtpPlugin>();
                });

            //载入配置
            service.Setup(config);

            //启动服务
            service.Start();

            Console.WriteLine($"{service.GetType()}已启动，管道：{name}，VerifyToken={service.VerifyToken}");
        }

        private static void CreateTLVTcpService(int port)
        {
            var tcpService = new TcpService();

            //订阅收到消息事件
            tcpService.Received += (client, e) =>
            {
                if (e.RequestInfo is TLVDataFrame frame)
                {
                    client.Send(frame);
                }

                return Task.CompletedTask;
            };

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(port) })
                 .SetContainer(GetContainer())
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

        private static void CreateUdpRpcParser(int port)
        {
            var udpDmtp = new UdpDmtp();

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost(port))
                 .SetContainer(GetContainer())
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();
                 })
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc()
                    .ConfigureRpcStore(a =>
                    {
                        a.RegisterServer<XUnitTestController>();
                        a.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });
                     a.UseDmtpFileTransfer();
                     a.UseDmtpRemoteStream();
                     a.UseDmtpRedis();
                     a.UseDmtpRemoteAccess();
                     a.UseDmtpRouterPackage();

                     a.Add<MyDmtpPlugin>();
                 });

            udpDmtp.Setup(config);

            udpDmtp.Start();

            Console.WriteLine($"UDPDmtp，端口号：{port}");
        }

        //private static void CreateUdpService(int bindPort, int targetPort)
        //{
        //    UdpSession udpSession = new UdpSession();
        //    udpSession.Received += (endpoint, byteBlock, requestInfo) =>
        //    {
        //        lock (udpSession)
        //        {
        //            udpSession.Send(endpoint, byteBlock);//将接收到的数据发送至默认终端
        //        }
        //    };

        //    TouchSocketConfig config = new TouchSocketConfig();
        //    config.SetBindIPHost(new IPHost($"127.0.0.1:{bindPort}"))
        //        .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())
        //        .SetBufferLength(1024 * 1024);

        //    udpSession.Setup(config);//加载配置
        //    udpSession.Start();//启动
        //    Console.WriteLine($"UdpService已启动，监听端口：{bindPort}，目标端口：{targetPort}");
        //}
    }

    class MyTcpPlugin : PluginBase, ITcpConnectedPlugin, ITcpDisconnectedPlugin
    {
        private readonly ILog m_logger;

        public MyTcpPlugin(ILog logger)
        {
            this.m_logger = logger;
        }
        public async Task OnTcpConnected(ITcpClientBase client, ConnectedEventArgs e)
        {
            m_logger.Info("TcpConnected");
            await e.InvokeNext();
        }

        public async Task OnTcpDisconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            m_logger.Info("TcpDisconnected");
            await e.InvokeNext();
        }
    }

    internal class MyWebSocketPlugin : PluginBase, IWebSocketReceivedPlugin, IWebSocketHandshakedPlugin
    {
        public async Task OnWebSocketHandshaked(IHttpClientBase client, HttpContextEventArgs e)
        {
            if (e.Context.Request.UrlEquals("/wsread"))
            {
                using (var webSocket = client.GetWebSocket())
                {
                    while (true)
                    {
                        using (var receiveResult = await webSocket.ReadAsync(CancellationToken.None))
                        {
                            if (receiveResult.DataFrame == null)
                            {
                                break;
                            }
                            await Console.Out.WriteLineAsync($"WebSocket同步Read：{receiveResult.DataFrame.ToText()}");
                            webSocket.Send(receiveResult.DataFrame.ToText());
                        }
                    }
                    client.Logger.Info("Websocket断开");
                }
                return;
            }
            await e.InvokeNext();
        }

        public async Task OnWebSocketReceived(IHttpClientBase client, WSDataFrameEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    internal class MyHttpPlugin : PluginBase, IHttpPlugin
    {
        public Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.IsGet())
            {
                if (e.Context.Request.UrlEquals("/xunit"))
                {
                    e.Handled = true;

                    e.Context.Response
                        .FromText("OK")
                        .SetStatus()
                        .Answer();
                    return Task.CompletedTask;
                }
            }
            return e.InvokeNext();
        }
    }

    class MyNamedPipePlugin : PluginBase, INamedPipeReceivedPlugin
    {
        async Task INamedPipeReceivedPlugin<INamedPipeClientBase>.OnNamedPipeReceived(INamedPipeClientBase client, ReceivedDataEventArgs e)
        {
            await client.SendAsync(e.ByteBlock.ToArray());
            await e.InvokeNext();
        }
    }

    internal class MyDmtpPlugin : PluginBase,
        IDmtpReceivedPlugin, IDmtpCreateChannelPlugin, IDmtpRoutingPlugin, IDmtpFileTransferingPlugin,
        IDmtpRemoteAccessingPlugin, IDmtpRemoteStreamPlugin, IDmtpRouterPackagePlugin
    {
        public async Task OnLoadingStream(IDmtpActorObject client, LoadingStreamEventArgs e)
        {
            if (e.Metadata?["1"] == "1")
            {
                e.IsPermitOperation = true;
                using (var stream = new MemoryStream())
                {
                    await e.WaitingLoadStreamAsync(stream, TimeSpan.FromSeconds(10));
                }
            }
            else if (e.Metadata?["2"] == "2")
            {
                e.IsPermitOperation = true;
                e.DataCompressor = new GZipDataCompressor();
                using (var stream = new MemoryStream())
                {
                    await e.WaitingLoadStreamAsync(stream, TimeSpan.FromSeconds(10));
                }
            }
            else
            {
                e.IsPermitOperation = false;
                e.Message = "fail";
            }

            await e.InvokeNext();
        }

        async Task IDmtpCreateChannelPlugin<IDmtpActorObject>.OnCreateChannel(IDmtpActorObject client, CreateChannelEventArgs e)
        {
            if (e.Metadata?["target"] == "server")
            {
                if (client.TrySubscribeChannel(e.ChannelId, out var channel))
                {
                    //while (channel.MoveNext())
                    //{
                    //    using (var byteBlock = channel.GetCurrent())
                    //    {
                    //        Console.WriteLine(byteBlock.Len);
                    //    }
                    //}
                    foreach (var byteBlock in channel)
                    {
                        Console.WriteLine(byteBlock.Len);
                        //using (byteBlock)
                        //{
                        //    Console.WriteLine(byteBlock.Len);
                        //}
                    }
                    Console.WriteLine($"通道接收结束，状态{channel.Status}");
                    if (channel.Status == ChannelStatus.Completed)
                    {
                        client.Send(10000);
                    }
                    //GC.Collect();
                    var size = BytePool.Default.GetPoolSize();
                    Console.WriteLine(size);

                    //BytePool.Default.Clear();
                    //GC.Collect();
                    //size = BytePool.Default.GetPoolSize();
                    //Console.WriteLine(size);
                }
            }


            await e.InvokeNext();
        }

        async Task IDmtpRouterPackagePlugin<IDmtpActorObject>.OnReceivedRouterPackage(IDmtpActorObject client, RouterPackageEventArgs e)
        {
            using (var byteBlock = new ByteBlock(1024 * 10))
            {
                byteBlock.SetLength(1024 * 10);
                var testPackage = new TestPackage()
                {
                    Message = "message",
                    Status = 1,
                    ByteBlock = byteBlock,
                };
                await e.ResponseAsync(testPackage);
            }
        }

        Task IDmtpRemoteAccessingPlugin<IDmtpActorObject>.OnRemoteAccessing(IDmtpActorObject client, RemoteAccessingEventArgs e)
        {
            e.IsPermitOperation = true;
            Console.WriteLine($"AccessMode={e.AccessMode},AccessType={e.AccessType}");

            return e.InvokeNext();
        }

        Task IDmtpFileTransferingPlugin<IDmtpActorObject>.OnDmtpFileTransfering(IDmtpActorObject client, FileTransferingEventArgs e)
        {
            e.IsPermitOperation = true;
            e.Message = "123";

            return e.InvokeNext();
        }

        Task IDmtpReceivedPlugin<IDmtpActorObject>.OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            var protocol = e.DmtpMessage.ProtocolFlags;
            ByteBlock byteBlock = e.DmtpMessage.BodyByteBlock;
            Console.WriteLine($"TcpDmtpService收到数据，协议为：{protocol}，数据长度为：{byteBlock.Len}");
            switch (protocol)
            {
                case 10000:
                    {
                        break;
                    }
                case 10001:
                    {
                        e.Handled = true;
                        var id = byteBlock.ReadString();
                        Task.Run(() =>
                        {
                            try
                            {
                                Console.WriteLine($"oldID={client.DmtpActor.Id},newID={id}");
                                client.DmtpActor.ResetId(id);
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
                        e.Handled = true;
                        break;
                    }
                default:
                    client.Send(protocol, byteBlock.ToArray(2));
                    break;
            }
            return Task.CompletedTask;
        }

        Task IDmtpRoutingPlugin<IDmtpActorObject>.OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
        {
            Console.WriteLine(e.RouterType);
            e.IsPermitOperation = true;

            return e.InvokeNext();
        }

        public class TestPackage : DmtpRouterPackage
        {
            public override int PackageSize => 1024 * 1024 * 2;

            public ByteBlock ByteBlock { get; set; }

            public override void PackageBody(in ByteBlock byteBlock)
            {
                base.PackageBody(byteBlock);
                byteBlock.WriteByteBlock(this.ByteBlock);
            }

            public override void UnpackageBody(in ByteBlock byteBlock)
            {
                base.UnpackageBody(byteBlock);
                this.ByteBlock = byteBlock.ReadByteBlock();
            }
        }
    }
}