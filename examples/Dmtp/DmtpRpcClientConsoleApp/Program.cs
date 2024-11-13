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

using RpcProxy;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ClientConsoleApp
{
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

            consoleAction.ShowAll();

            await consoleAction.RunCommandLineAsync();
        }

        private static void ConsoleAction_OnException(Exception obj)
        {
            ConsoleLogger.Default.Exception(obj);
        }

        private static async Task RunInvokeCancellationToken()
        {
            var client = await GetTcpDmtpClient();

            //设置调用配置。当设置可取消操作时，invokeOption必须每次重新new，然后对invokeOption.Token重新赋值。

            //创建一个指定时间可取消令箭源，可用于取消Rpc的调用。
            using (var tokenSource = new CancellationTokenSource(5000))
            {
                var invokeOption = new DmtpInvokeOption()//调用配置
                {
                    FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
                    SerializationType = SerializationType.FastBinary,//序列化类型
                    Timeout = 5000,//调用超时设置
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

        private static async Task RunRpcPullChannel()
        {
            using var client = await GetTcpDmtpClient();
            var status = ChannelStatus.Default;
            var size = 0;
            var channel = client.CreateChannel();//创建通道
            var task = Task.Run(() =>//这里必须用异步
            {
                using (channel)
                {
                    foreach (var currentByteBlock in channel)
                    {
                        size += currentByteBlock.Length;//此处可以处理传递来的流数据
                    }
                    status = channel.Status;//最后状态
                }
            });
            var result = await client.GetDmtpRpcActor().RpcPullChannelAsync(channel.Id);//RpcPullChannel是代理方法，此处会阻塞至服务器全部发送完成。
            await task;//等待异步接收完成
            Console.WriteLine($"状态：{status}，size={size}");
        }

        private static async Task RunRpcPushChannel()
        {
            using var client = await GetTcpDmtpClient();
            var status = ChannelStatus.Default;
            var size = 0;
            var package = 1024;
            var channel =await client.CreateChannelAsync();//创建通道
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
            var result =await client.GetDmtpRpcActor().RpcPushChannelAsync(channel.Id);//RpcPushChannel是代理方法，此处会阻塞至服务器全部完成。
            await task;//等待异步接收完成
            Console.WriteLine($"状态：{status}，result={result}");
        }

        private static async Task<TcpDmtpClient> GetTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();
                     a.AddRpcStore(store =>
                     {
                         store.RegisterServer<MyClientRpcServer>();
                     });
                 })
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc()
                     //.SetSerializationSelector(new MySerializationSelector())//自定义序列化器
                     .SetCreateDmtpRpcActor((actor, serverprovider, resolver) => new MyDmtpRpcActor(actor, serverprovider, resolver));

                     a.UseDmtpHeartbeat()
                     .SetTick(TimeSpan.FromSeconds(3))
                     .SetMaxFailCount(3);
                 })
                 .SetRemoteIPHost("127.0.0.1:7789")
                 .SetDmtpOption(new DmtpOption()
                 {
                     VerifyToken = "Dmtp"
                 }));
            await client.ConnectAsync();

            var rpcClient1 = client.GetDmtpRpcActor<IRpcClient1>();
            var rpcClient2 = client.GetDmtpRpcActor<IRpcClient2>();

            client.Logger.Info($"连接成功，Id={client.Id}");
            return client;
        }
    }

    internal interface IRpcClient1 : IDmtpRpcActor
    {
    }

    internal interface IRpcClient2 : IDmtpRpcActor
    {
    }

    internal class MyDmtpRpcActor : DmtpRpcActor, IRpcClient1, IRpcClient2
    {
        public MyDmtpRpcActor(IDmtpActor smtpActor, IRpcServerProvider rpcServerProvider, IResolver resolver) : base(smtpActor, rpcServerProvider, resolver)
        {
        }
    }

    internal partial class MyClientRpcServer : RpcServer
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

    /// <summary>
    /// 序列化选择器
    /// </summary>
    public class MySerializationSelector : ISerializationSelector
    {
        public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IByteBlock
        {
            throw new NotImplementedException();
        }

        public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IByteBlock
        {
            throw new NotImplementedException();
        }
    }
}