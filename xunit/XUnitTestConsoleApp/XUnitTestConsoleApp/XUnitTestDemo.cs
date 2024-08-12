//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RpcArgsClassLib;
using TouchSocket.Core;
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
using System.Net.WebSockets;

namespace XUnitTestConsoleApp
{
    public static class XUnitTestDemo
    {
        private static HttpDmtpService m_httpService;

        private static IRegistrator GetContainer()
        {
            var container = new Container();
            container.AddLogger(loggerGroup =>
            {
                loggerGroup.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
            });
            return container;

            //var container = new AspNetCoreContainer(new ServiceCollection());
            //container.AddLogger(loggerGroup =>
            //{
            //    loggerGroup.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
            //});
            //return container;

            //var container = new AutofacContainer(new ContainerBuilder());
            //container.AddLogger(loggerGroup =>
            //{
            //    loggerGroup.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
            //});
            //return container;
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
            //CreateMultipathTcpService(7802);
            CreateTcpJsonRpcParser(7803);

            CreateTLVTcpService(7805);

            CreatePeriodTcpService(7807);


            CodeGenerator.AddProxyType<ProxyClass1>();
            CodeGenerator.AddProxyType<ProxyClass2>(deepSearch: true);

            var serverCellCodes = CodeGenerator.GetProxyCodes("RpcProxy",
                new Type[] { typeof(XUnitTestController), typeof(IOtherAssemblyServer) }, new Type[]
                {typeof(XunitDmtpRpcAttribute),typeof(XunitJsonRpcAttribute),typeof(XunitWebApiAttribute),typeof(XunitXmlRpcAttribute) });
            File.WriteAllText("../../../XunitRpcProxy.cs", serverCellCodes);

            {
                var builder = WebApplication.CreateBuilder();

                builder.Services.AddTcpService<ITcpService, TcpService>(config =>
                {
                    config.ConfigureContainer(container =>
                    {

                    });

                    config.ConfigurePlugins(a =>
                    {
                        a.UseCheckClear();
                    });
                });

                builder.Services.AddWebSocketDmtpService(config =>
                {
                    config.SetDmtpOption(new DmtpOption()
                    {
                        VerifyToken = "123RPC"
                    })
                         .UseAspNetCoreContainer(builder.Services)
                         .ConfigureContainer(a =>
                         {
                             a.AddConsoleLogger();
                             a.AddDmtpRouteService();
                             a.AddRpcStore(store =>
                             {
                                 store.RegisterServer<XUnitTestController>();
                                 store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();

                                 string core = store.GetProxyCodes("ProxyCore", new Type[] { typeof(DmtpRpcAttribute) });
                             });
                         })
                         .ConfigurePlugins(a =>
                         {
                             a.UseDmtpRpc();
                             a.UseDmtpFileTransfer();
                             a.UseDmtpRemoteStream();
                             a.UseDmtpRedis();
                             a.UseDmtpRemoteAccess();
                             a.UseDmtpRouterPackage();

                             a.Add<MyDmtpPlugin>();
                         });
                });

                builder.Services.AddHttpMiddlewareDmtpService(config =>
                {
                    config
                    .SetDmtpOption(new DmtpOption()
                    {
                        VerifyToken = "123RPC"
                    })
                    .ConfigurePlugins(a =>
                    {
                        a.UseDmtpRpc();
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

        //private static void CreateMultipathTcpService(int port)
        //{
        //    var service = new MultipathTcpService();

        //    var config = new TouchSocketConfig();
        //    config.SetListenIPHosts(port)
        //        .SetContainer(GetContainer())
        //        .ConfigurePlugins(a =>
        //        {
        //            a.UseCheckClear();
        //        });

        //    //载入配置
        //    service.Setup(config);

        //    //启动
        //    service.Start();
        //    Console.WriteLine($"{service.GetType().Name}已启动,端口：{port}");
        //}

        private static void CreateHttpService(int port)
        {
            m_httpService = new HttpDmtpService();
            m_httpService.Setup(new TouchSocketConfig()
                 .SetRegistrator(GetContainer())
                 .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                 .SetDmtpOption(new DmtpOption()
                 {
                     VerifyToken = "123RPC"
                 })
                 .ConfigureContainer(a =>
                 {
                     a.AddDmtpRouteService();
                     a.AddRpcStore(store =>
                     {
                         store.RegisterServer<XUnitTestController>();
                         store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                     });
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

                    a.UseWebApi();

                    //a.UseSwagger()
                    //.UseLaunchBrowser();

                    a.UseWebSocketJsonRpc()
                    .SetAllowJsonRpc((client, context) =>
                    {
                        if (context.Request.UrlEquals("/wsjsonrpc"))
                        {
                            return Task.FromResult(true);
                        }
                        return Task.FromResult(false);
                    });

                    a.UseHttpJsonRpc()
                    .SetJsonRpcUrl("/jsonRpc");

                    a.UseXmlRpc()
                    .SetXmlRpcUrl("/xmlrpc");

                    a.UseDmtpRpc();
                    a.UseDmtpFileTransfer();
                    a.UseDmtpRemoteStream();
                    a.UseDmtpRedis();
                    a.UseDmtpRemoteAccess();
                    a.UseDmtpRouterPackage();

                    a.Add<MyDmtpPlugin>();
                    a.Add<MyHttpPlugin>();
                }));
            m_httpService.Start();
            Console.WriteLine($"HttpService已启动,端口：{port}");
        }

        private static void CreateTcpJsonRpcParser(int port)
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()
                 .SetRegistrator(GetContainer())
                 .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .SetListenIPHosts(port)
                .ConfigureContainer(a =>
                {
                    a.AddRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });
                })
                .ConfigurePlugins(a =>
                {
                    a.UseTcpJsonRpc();
                }));

            service.Start();
        }

        private static NamedPipeService CreateNamedPipeService(string name)
        {
            var service = new NamedPipeService();
            var config = new TouchSocketConfig();
            config.SetPipeName(name)
                .SetRegistrator(GetContainer())
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
                .SetRegistrator(GetContainer())
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
                .SetRegistrator(GetContainer())
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "123RPC"
                })
                .ConfigureContainer(a =>
                {
                    a.AddDmtpRouteService();
                    a.AddRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
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
                .SetRegistrator(GetContainer())
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "123RPC"
                })
                .ConfigureContainer(a =>
                {
                    a.AddDmtpRouteService();
                    a.AddRpcStore(store =>
                    {
                        store.RegisterServer<XUnitTestController>();
                        store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                    });
                })
                .ConfigurePlugins(a =>
                {
                    //a.UseAutoBufferLength();

                    a.UseDmtpRpc();
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
                 .SetRegistrator(GetContainer())
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

        private static void CreatePeriodTcpService(int port)
        {
            var tcpService = new TcpService();

            //订阅收到消息事件
            tcpService.Received = (client, e) =>
            {
                Console.WriteLine(e.ByteBlock.ToString());
                return Task.CompletedTask;
            };

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(port) })
                 .SetRegistrator(GetContainer())
                 .SetTcpDataHandlingAdapter(() => new PeriodPackageAdapter())
                .ConfigurePlugins(a =>
                {

                });

            //载入配置
            tcpService.Setup(config);

            //启动
            tcpService.Start();
            Console.WriteLine($"PeriodTcpService已启动,端口：{port}");
        }

        private static void CreateUdpRpcParser(int port)
        {
            var udpDmtp = new UdpDmtp();

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost(port))
                 .SetRegistrator(GetContainer())
                 .ConfigureContainer(a =>
                 {
                     a.AddRpcStore(store =>
                     {
                         store.RegisterServer<XUnitTestController>();
                         store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                     });
                 })
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc();
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
        public async Task OnWebSocketHandshaked(IWebSocket client, HttpContextEventArgs e)
        {
            if (e.Context.Request.UrlEquals("/wsread"))
            {
                client.AllowAsyncRead = true;
                while (true)
                {
                    using (var receiveResult = await client.ReadAsync(CancellationToken.None))
                    {
                        if (receiveResult.DataFrame == null)
                        {
                            break;
                        }
                        await Console.Out.WriteLineAsync($"WebSocket同步Read：{receiveResult.DataFrame.ToText()}");
                        client.Send(receiveResult.DataFrame.ToText());
                    }
                }
                client.Client.Logger.Info("Websocket断开");
                return;
            }
            await e.InvokeNext();
        }

        public async Task OnWebSocketReceived(IWebSocket client, WSDataFrameEventArgs e)
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

                    using (channel)
                    {
                        int i = 0;
                        var size = 0;
                        foreach (var byteBlock in channel)
                        {
                            byteBlock.SeekToStart();
                            var num = byteBlock.ReadInt32(EndianType.Big);
                            if (num != i)
                            {
                                Console.WriteLine("顺序不对");
                                return;
                            }
                            //Console.WriteLine(i);
                            size += byteBlock.Len;
                            i++;
                        }
                        Console.WriteLine($"通道接收结束，状态{channel.Status}，长度={size}");
                        if (channel.Status == ChannelStatus.Completed)
                        {
                            client.Send(10000);
                        }
                        //GC.Collect();
                        //var size = BytePool.Default.GetPoolSize();
                        //Console.WriteLine(size);

                        //BytePool.Default.Clear();
                        //GC.Collect();
                        //size = BytePool.Default.GetPoolSize();
                        //Console.WriteLine(size);
                    }
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
            Console.WriteLine($"DmtpService收到数据，协议为：{protocol}，数据长度为：{byteBlock.Len}");
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

        async Task IDmtpRoutingPlugin<IDmtpActorObject>.OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
        {
            Console.WriteLine(e.RouterType);
            e.IsPermitOperation = true;
            await e.InvokeNext();
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