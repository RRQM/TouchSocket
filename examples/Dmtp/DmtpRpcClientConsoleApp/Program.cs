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

using MemoryPack;
using RpcProxy;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ClientConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;

        consoleAction.Add("1", "直接调用Rpc", RunInvokeT);
        consoleAction.Add("2", "客户端互相调用Rpc", RunInvokeT_2C);
        consoleAction.Add("3", "测试客户端请求，服务器响应大量流数据", RunRpcPullChannel);
        consoleAction.Add("4", "测试客户端推送大量流数据", RunRpcPushChannel);
        consoleAction.Add("5", "测试取消调用", RunInvokeCancellationToken);
        consoleAction.Add("6", "测试从CallContextAccessor中获取当前关联的CallContext", RunInvokeGetCallContextFromCallContextAccessor);

        consoleAction.ShowAll();

        await consoleAction.RunCommandLineAsync();
    }

    private static void ConsoleAction_OnException(Exception obj)
    {
        ConsoleLogger.Default.Exception(obj);
    }

    private static async Task RunInvokeGetCallContextFromCallContextAccessor()
    {
        var client = await GetTcpDmtpClient();

        try
        {
            await client.GetDmtpRpcActor().TestGetCallContextFromCallContextAccessorAsync();
            Console.WriteLine("success");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    private static async Task RunInvokeCancellationToken()
    {
        var client = await GetTcpDmtpClient();

        //设置调用配置。当设置可取消操作时，invokeOption必须每次重新new，然后对invokeOption.Token重新赋值。

        //创建一个指定时间可取消令箭源，可用于取消Rpc的调用。
        using (var tokenSource = new CancellationTokenSource(5000))
        {
            var invokeOption = new DmtpInvokeOption(5000)//调用配置
            {
                FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
                SerializationType = SerializationType.FastBinary,//序列化类型
                Token = tokenSource.Token//配置可取消令箭
            };

            try
            {
                var result = await client.GetDmtpRpcActor().TestCancellationTokenAsync(invokeOption);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// 客户端互相调用Rpc
    /// </summary>
    private static async Task RunInvokeT_2C()
    {
        var client1 = await GetTcpDmtpClient();
        var client2 = await GetTcpDmtpClient();

        await client1.GetDmtpRpcActor().InvokeTAsync<bool>(client2.Id, "Notice", InvokeOption.WaitInvoke, "Hello");

        //使用下面方法targetRpcClient也能使用代理调用。
        var targetRpcClient = client1.CreateTargetDmtpRpcActor(client2.Id);
        await targetRpcClient.InvokeTAsync<bool>("Notice", InvokeOption.WaitInvoke, "Hello");
    }

    /// <summary>
    /// 直接调用Rpc
    /// </summary>
    private static async Task RunInvokeT()
    {
        var client = await GetTcpDmtpClient();

        #region DmtpRpc直接调用
        //设置调用配置
        using var cts = new CancellationTokenSource(5000);//可取消令箭源，可用于取消Rpc的调用
        var invokeOption = new DmtpInvokeOption()//调用配置
        {
            FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
            SerializationType = SerializationType.FastBinary,//序列化类型
            Token = cts.Token//配置可取消令箭
        };
        //获取RpcActor，用于后续的rpc调用
        var rpcActor = client.GetDmtpRpcActor();

        //调用Add方法
        var sum = await rpcActor.InvokeTAsync<int>("Add", invokeOption, 10, 20);
        client.Logger.Info($"调用Add方法成功，结果：{sum}");
        #endregion

    }

    private static async Task RunInvokeWithProxy()
    {
        using var client = await GetTcpDmtpClient();

        #region DmtpRpc代理调用
        //设置调用配置
        using var cts = new CancellationTokenSource(5000);//可取消令箭源，可用于取消Rpc的调用
        var invokeOption = new DmtpInvokeOption()//调用配置
        {
            FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
            SerializationType = SerializationType.FastBinary,//序列化类型
            Token = cts.Token//配置可取消令箭
        };
        //获取RpcActor，用于后续的rpc调用
        var rpcActor = client.GetDmtpRpcActor();

        //调用Add方法
        var sum = await rpcActor.AddAsync(10, 20, invokeOption);
        client.Logger.Info($"调用Add方法成功，结果：{sum}");
        #endregion

    }

    #region DmtpRpc客户端请求流数据
    private static async Task RunRpcPullChannel()
    {
        using var client = await GetTcpDmtpClient();
        var status = ChannelStatus.Default;
        var size = 0;
        var channel = await client.CreateChannelAsync();//创建通道
        var task = Task.Run(async () =>//这里必须用异步
        {
            using (channel)
            {
                while (channel.CanRead)
                {
                    using var cts = new CancellationTokenSource(10 * 1000);
                    var memory = await channel.ReadAsync(cts.Token);
                    size += memory.Length;
                }
                status = channel.Status;//最后状态
            }
        });
        var result = await client.GetDmtpRpcActor().RpcPullChannelAsync(channel.Id);//RpcPullChannel是代理方法，此处会阻塞至服务器全部发送完成。
        await task;//等待异步接收完成
        Console.WriteLine($"状态：{status}，size={size}");
    }
    #endregion

    #region DmtpRpc客户端推送流数据
    private static async Task RunRpcPushChannel()
    {
        using var client = await GetTcpDmtpClient();
        var status = ChannelStatus.Default;
        var size = 0;
        var package = 1024;
        var channel = await client.CreateChannelAsync();//创建通道
        var task = Task.Run(async () =>//这里必须用异步
        {
            for (var i = 0; i < 1024; i++)
            {
                size += package;
                await channel.WriteAsync(new byte[package]);
            }
            await channel.CompleteAsync();//必须调用指令函数，如Complete，Cancel，Dispose

            status = channel.Status;
        });
        var result = await client.GetDmtpRpcActor().RpcPushChannelAsync(channel.Id);//RpcPushChannel是代理方法，此处会阻塞至服务器全部完成。
        await task;//等待异步接收完成
        Console.WriteLine($"状态：{status}，result={result}");
    }
    #endregion

    private static async Task CreateDmtpRpcClient()
    {
        #region 创建DmtpRpc客户端
        var client = new TcpDmtpClient();
        await client.SetupAsync(new TouchSocketConfig()
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  //启用dmtp rpc插件
                  a.UseDmtpRpc();
              })
              .SetRemoteIPHost("127.0.0.1:7789")
              .SetDmtpOption(options =>
              {
                  options.VerifyToken = "Dmtp";
              }));
        await client.ConnectAsync();
        #endregion
    }

    private static async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var client = new TcpDmtpClient();
        await client.SetupAsync(new TouchSocketConfig()

        #region 客户端注册反向DmtpRpc服务
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<MyClientRpcServer>();
                });
            })
        #endregion
             .ConfigurePlugins(a =>
             {
                 a.UseDmtpRpc(options =>
                 {
                     options.SetCreateDmtpRpcActor((actor, serverprovider, dispatcher) => new MyDmtpRpcActor(actor, serverprovider, dispatcher));
                 });
             })
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetDmtpOption(options =>
             {
                 options.VerifyToken = "Dmtp";
             }));
        await client.ConnectAsync();

        var rpcClient1 = client.GetDmtpRpcActor<IRpcClient1>();
        var rpcClient2 = client.GetDmtpRpcActor<IRpcClient2>();

        client.Logger.Info($"连接成功，Id={client.Id}");
        return client;
    }
}

#region DmtpRpc限制代理接口声明
internal interface IRpcClient1 : IDmtpRpcActor
{
}

internal interface IRpcClient2 : IDmtpRpcActor
{
}

internal class MyDmtpRpcActor : DmtpRpcActor, IRpcClient1, IRpcClient2
{
    public MyDmtpRpcActor(IDmtpActor dmtpActor, IRpcServerProvider rpcServerProvider, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext> dispatcher) : base(dmtpActor, rpcServerProvider, dmtpActor.Client.Resolver, dispatcher)
    {
    }
}
#endregion

#region DmtpRpc限制代理接口配置
internal class LimitInterfaceExample
{
    public async Task ConfigLimitInterface()
    {
        var client = new TcpDmtpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc(options =>
                {
                    options.SetCreateDmtpRpcActor((actor, serverprovider, dispatcher) => new MyDmtpRpcActor(actor, serverprovider, dispatcher));
                });
            })
            .SetRemoteIPHost("127.0.0.1:7789")
            .SetDmtpOption(options =>
            {
                options.VerifyToken = "Rpc";//连接验证口令。
            }));
        await client.ConnectAsync();
    }
}
#endregion

#region DmtpRpc限制代理接口使用
internal class UseLimitInterfaceExample
{
    public void UseLimitInterface(TcpDmtpClient client)
    {
        IRpcClient1 rpcClient1 = client.GetDmtpRpcActor<IRpcClient1>();
        IRpcClient2 rpcClient2 = client.GetDmtpRpcActor<IRpcClient2>();
    }
}
#endregion

#region 声明反向DmtpRpc服务
internal partial class MyClientRpcServer : SingletonRpcServer
{
    private readonly ILog m_logger;

    public MyClientRpcServer(ILog logger)
    {
        this.m_logger = logger;
    }

    [DmtpRpc(MethodInvoke = true)]//使用函数名直接调用
    public bool Notice(string msg)
    {
        this.m_logger.Info(msg);
        return true;
    }
}

#endregion

/// <summary>
/// 序列化选择器
/// </summary>
public class MySerializationSelector : ISerializationSelector
{
    public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IBytesReader
    {
        throw new NotImplementedException();
    }

    public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IBytesWriter
    {
        throw new NotImplementedException();
    }
}

#region DmtpRpc配置路由
internal class ConfigureRouteExample
{
    public void ConfigureRoute()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.AddDmtpRouteService();
                a.AddConsoleLogger();
            });
    }
}
#endregion

#region DmtpRpc设置最大包大小
internal class SetMaxPackageSizeExample
{
    public void ConfigMaxPackageSize()
    {
        var config = new TouchSocketConfig();//配置
        config.SetAdapterOption(options =>
        {
            options.MaxPackageSize = 10 * 1024 * 1024;//设置最大包大小为10MB
        });
    }
}
#endregion

#region DmtpRpc配置序列化选择器
internal class ConfigureSerializationSelectorExample
{
    public void ConfigSerializationSelector()
    {
        var config = new TouchSocketConfig()
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc(options =>
                {
                    options.ConfigureDefaultSerializationSelector(selector =>
                    {
                        selector.JsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                        selector.FastSerializerContext = default;
                    });
                });
            });
    }
}
#endregion

#region DmtpRpc自定义序列化配置
internal class CustomSerializationExample
{
    public void ConfigCustomSerialization()
    {
        var config = new TouchSocketConfig()
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc(options =>
                {
                    options.SerializationSelector = new MemoryPackSerializationSelector();
                });
            });
    }
}

public class MemoryPackSerializationSelector : ISerializationSelector
{
    public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IBytesReader
    {
        var len = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
        var span = ReaderExtension.ReadToSpan(ref byteBlock, len);
        return MemoryPackSerializer.Deserialize(parameterType, span);
    }

    public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IBytesWriter
    {
        var writerAnchor = new WriterAnchor<TByteBlock>(ref byteBlock, 4);

        var memoryPackWriter = new MemoryPackWriter<TByteBlock>(ref byteBlock, null);

        MemoryPackSerializer.Serialize(parameter.GetType(), ref memoryPackWriter, parameter);

        var span = writerAnchor.Rewind(ref byteBlock, out var len);
        span.WriteValue(memoryPackWriter.WrittenCount);
    }
}
#endregion

#region DmtpRpc使用自定义序列化类型
internal class UseCustomSerializationTypeExample
{
    public async Task UseCustomSerializationType()
    {
        var client = new TcpDmtpClient();
        var invokeOption = new DmtpInvokeOption()//调用配置
        {
            FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
            SerializationType = (SerializationType)4,//序列化类型
            Timeout = 5000,//调用超时设置
        };
    }
}
#endregion

#region DmtpRpc使用Metadata
internal partial class MetadataRpcServer : SingletonRpcServer
{
    [DmtpRpc(MethodInvoke = true)]
    public Metadata CallContextMetadata(IDmtpRpcCallContext callContext)
    {
        return callContext.Metadata;
    }
}
#endregion

#region DmtpRpc客户端使用Metadata
internal class ClientUseMetadataExample
{
    public async Task UseMetadata()
    {
        var client = new TcpDmtpClient();
        var invokeOption = new DmtpInvokeOption()//调用配置
        {
            FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
            SerializationType = SerializationType.FastBinary,//序列化类型
            Timeout = 5000,//调用超时设置
            Metadata = new Metadata() { { "a", "a" } }
        };

        var metadata = await client.GetDmtpRpcActor().InvokeTAsync<Metadata>("CallContextMetadata", invokeOption);
    }
}
#endregion

#region DmtpRpc使用CancellationToken
internal class UseCancellationTokenExample
{
    public async Task UseCancellationToken()
    {
        var client = new TcpDmtpClient();

        //设置调用配置
        var tokenSource = new CancellationTokenSource();//可取消令箭源，可用于取消Rpc的调用
        var invokeOption = new DmtpInvokeOption()//调用配置
        {
            FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
            SerializationType = SerializationType.FastBinary,//序列化类型
            Timeout = 5000,//调用超时设置
            Token = tokenSource.Token//配置可取消令箭
        };

        var sum = await client.GetDmtpRpcActor().InvokeTAsync<int>("Add", invokeOption, 10, 20);
        client.Logger.Info($"调用Add方法成功，结果：{sum}");
    }
}
#endregion

#region DmtpRpc客户端互Call示例
internal class ClientToClientCallExample
{
    public async Task CallBetweenClients()
    {
        var client1 = new TcpDmtpClient();
        var client2 = new TcpDmtpClient();

        await client1.GetDmtpRpcActor().InvokeTAsync<bool>(client2.Id, "Notice", InvokeOption.WaitInvoke, "Hello");
    }
}
#endregion

#region DmtpRpc客户端互Call使用目标RpcActor
internal class ClientToClientCallWithTargetExample
{
    public async Task CallWithTarget()
    {
        var client1 = new TcpDmtpClient();
        var client2 = new TcpDmtpClient();

        var targetRpcClient = client1.CreateTargetDmtpRpcActor(client2.Id);
        await targetRpcClient.InvokeTAsync<bool>("Notice", InvokeOption.WaitInvoke, "Hello");
    }
}
#endregion
