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
using RpcArgsClassLib;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Dmtp;
using TouchSocket.NamedPipe;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using XUnitTestConsoleApp.Server;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.RemoteStream;
using TouchSocket.Dmtp.Redis;
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Dmtp.RouterPackage;
using TouchSocket.Http;
using TouchSocket.WebApi.Swagger;
using TouchSocket.Modbus;
using TouchSocket.SerialPorts;
using TouchSocket.Rpc.RateLimiting;
using System.Net;

namespace XUnitTestConsoleApp
{
    internal class XUnitTestDemoHostBuilder
    {
        public static void Start()
        {

            CodeGenerator.AddProxyType<ProxyClass1>();
            CodeGenerator.AddProxyType<ProxyClass2>(deepSearch: true);

            var builder = WebApplication.CreateBuilder();

            builder.Services.ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
                a.AddDmtpRouteService();
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<XUnitTestController>();
                    store.RegisterServer<IOtherAssemblyServer, OtherAssemblyServer>();
                });

                a.AddCors(options =>
                {
                    options.Add("cors", builder =>
                    {
                        builder.AllowAnyHeaders()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                    });
                });

                a.AddRateLimiter(p =>
                {
                    p.AddFixedWindowLimiter("FixedWindow", option =>
                    {
                        option.PermitLimit = 2;
                        option.Window = TimeSpan.FromSeconds(5);
                    });
                });
            });

            builder.Services.AddServiceHostedService<INamedPipeService, NamedPipeService>(config =>
            {
                config.SetPipeName("pipe7789")
                   .ConfigurePlugins(a =>
                   {
                       a.Add<MyNamedPipePlugin>();
                   });
            });

            builder.Services.AddServiceHostedService<INamedPipeDmtpService, NamedPipeDmtpService>(config =>
            {
                config.SetPipeName("pipe7790")
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "123RPC"
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
            });

            builder.Services.AddTcpService(config =>
            {
                config.SetListenIPHosts(7789)
                    .ConfigurePlugins(a =>
                    {
                        a.Add(nameof(ITcpReceivedPlugin.OnTcpReceived), async (SocketClient client, ReceivedDataEventArgs e) =>
                        {
                            await client.SendAsync(e.ByteBlock);
                        });
                        a.UseCheckClear();
                        a.Add<MyTcpPlugin>();
                    });
            });

            builder.Services.AddHttpDmtpService(config =>
            {
                config.SetListenIPHosts(7801)
                     .SetDmtpOption(new DmtpOption()
                     {
                         VerifyToken = "123RPC"
                     })
                    .ConfigurePlugins(a =>
                    {
                        a.UseCors("cors");

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
                        .ConfigureConverter(converter => 
                        {
                            converter.AddXmlSerializerFormatter();
                        });

                        //a.UseSwagger()
                        //.UseLaunchBrowser();

                        a.UseSwagger();

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
                    });
            });

            builder.Services.AddTcpDmtpService(config =>
            {
                config.SetListenIPHosts(7794)
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

            builder.Services.AddServiceHostedService<IUdpDmtp, UdpDmtp>(config =>
            {
                config.SetBindIPHost(7797)
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

            builder.Services.AddTcpService<ITcpJsonRpcService, TcpJsonRpcService>(config =>
            {
                config.SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .SetListenIPHosts(7803)
                .ConfigurePlugins(a =>
                {
                    a.UseTcpJsonRpc();
                });
            });

            builder.Services.AddTcpService<ITcpTLVService, TcpTLVService>(config =>
            {
                config.SetListenIPHosts(7805)
                    .ConfigurePlugins(a =>
                    {
                        a.UseTLV();

                        a.Add(nameof(ITcpReceivedPlugin.OnTcpReceived), async (SocketClient client, ReceivedDataEventArgs e) =>
                        {
                            if (e.RequestInfo is TLVDataFrame frame)
                            {
                                await client.SendAsync(frame);
                            }
                        });
                    });
            });

            builder.Services.AddWebSocketDmtpService(config =>
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

            builder.Services.AddTransientTcpClient(config =>
            {
                config.SetRemoteIPHost("127.0.0.1:7789");
            });

            builder.Services.AddServiceHostedService<IModbusTcpSlave, ModbusTcpSlave>(config =>
            {
                config.SetListenIPHosts(7808)
                .ConfigurePlugins(a =>
                {
                    a.AddModbusSlavePoint()
                    .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));
                });
            });

            //builder.Services.AddSingletonSetupConfigObject<IModbusRtuSlave, ModbusRtuSlave>(config =>
            //{
            //    config.SetSerialPortOption(new SerialPortOption()
            //    {
            //        BaudRate = 9600,
            //        DataBits = 8,
            //        Parity = System.IO.Ports.Parity.Even,
            //        PortName = "COM1",
            //        StopBits = System.IO.Ports.StopBits.One
            //    })
            //    .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));
            //});

            builder.Services.AddServiceHostedService<IModbusUdpSlave, ModbusUdpSlave>(config =>
            {
                config.SetBindIPHost(7809)
                .ConfigurePlugins(a =>
                {
                    a.AddModbusSlavePoint()
                    .SetModbusDataLocater(new ModbusDataLocater()
                    {
                        Coils = new BooleanDataPartition(1000, 10),
                        DiscreteInputs = new BooleanDataPartition(1000, 10),
                        HoldingRegisters = new ShortDataPartition(1000, 10),
                        InputRegisters = new ShortDataPartition(1000, 10)
                    });
                });
            });

            builder.Services.AddServiceHostedService<IModbusRtuOverTcpSlave, ModbusRtuOverTcpSlave>(config =>
            {
                config.SetListenIPHosts(7810)
                .ConfigurePlugins(a =>
                {
                    a.AddModbusSlavePoint()
                    .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));
                });
            });

            builder.Services.AddServiceHostedService<IModbusRtuOverUdpSlave, ModbusRtuOverUdpSlave>(config =>
            {
                config.SetBindIPHost(7811)
                .ConfigurePlugins(a =>
                {
                    a.AddModbusSlavePoint()
                    .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));
                });
            });

            builder.Services.AddTcpService<ITcpIpv6Service, TcpIpv6Service>(config =>
            {
                config.SetListenIPHosts("[::]:7812")
                    .ConfigurePlugins(a =>
                    {
                        a.Add(nameof(ITcpReceivedPlugin.OnTcpReceived), async (SocketClient client, ReceivedDataEventArgs e) =>
                        {
                            await client.SendAsync(e.ByteBlock);
                        });
                    });
            });

            var app = builder.Build();

            app.UseWebSockets();
            app.UseWebSocketDmtp();

            app.UseHttpDmtp();

            app.RunAsync("http://127.0.0.1:7806");

            //app.Services.GetRequiredService<IModbusRtuSlave>().Connect();
            var modbusTcpSlave = app.Services.GetRequiredService<IModbusTcpSlave>();

            var slavePoint = modbusTcpSlave.GetSlavePointBySlaveId(1);
            var localMaster = slavePoint.ModbusDataLocater.CreateDataLocaterMaster();
            var v = localMaster.ReadCoils(0, 1);
            while (true)
            {
                var pool = BytePool.Default;
                Console.WriteLine(pool.GetPoolSize());
                Console.ReadKey();
            }
        }
    }

    public interface ITcpJsonRpcService : ITcpService
    {

    }

    public class TcpJsonRpcService : TcpService, ITcpJsonRpcService
    {

    }

    public interface ITcpTLVService : ITcpService
    {

    }

    public class TcpTLVService : TcpService, ITcpTLVService
    {

    }

    public interface ITcpIpv6Service : ITcpService
    {

    }

    public class TcpIpv6Service : TcpService, ITcpIpv6Service
    {

    }


}