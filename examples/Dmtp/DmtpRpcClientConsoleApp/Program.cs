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
        static void Main(string[] args)
        {
            var consoleAction = new ConsoleAction();
            consoleAction.OnException += ConsoleAction_OnException;

            consoleAction.Add("1", "直接调用Rpc", RunInvokeT);
            consoleAction.Add("2", "客户端互相调用Rpc", RunInvokeT_2C);
            consoleAction.Add("3", "测试客户端请求，服务器响应大量流数据", () => { RunRpcPullChannel(); });
            consoleAction.Add("4", "测试客户端推送大量流数据", () => { RunRpcPushChannel(); });

            consoleAction.ShowAll();

            while (true)
            {
                if (!consoleAction.Run(Console.ReadLine()))
                {
                    consoleAction.ShowAll();
                }
            }
        }


        private static void ConsoleAction_OnException(Exception obj)
        {
            ConsoleLogger.Default.Exception(obj);
        }

        /// <summary>
        /// 客户端互相调用Rpc
        /// </summary>
        static void RunInvokeT_2C()
        {
            var client1 = GetTcpDmtpClient();
            var client2 = GetTcpDmtpClient();

            client1.GetDmtpRpcActor().InvokeT<bool>(client2.Id, "Notice", InvokeOption.WaitInvoke, "Hello");

            //使用下面方法targetRpcClient也能使用代理调用。
            var targetRpcClient = client1.CreateTargetDmtpRpcActor(client2.Id);
            targetRpcClient.InvokeT<bool>("Notice", InvokeOption.WaitInvoke, "Hello");
        }

        /// <summary>
        /// 直接调用Rpc
        /// </summary>
        static void RunInvokeT()
        {
            var client = GetTcpDmtpClient();

            //设置调用配置
            var tokenSource = new CancellationTokenSource();//可取消令箭源，可用于取消Rpc的调用
            var invokeOption = new DmtpInvokeOption()//调用配置
            {
                FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
                SerializationType = SerializationType.FastBinary,//序列化类型
                Timeout = 5000,//调用超时设置
                Token = tokenSource.Token//配置可取消令箭
            };

            var sum = client.GetDmtpRpcActor().InvokeT<int>("Add", invokeOption, 10, 20);
            client.Logger.Info($"调用Add方法成功，结果：{sum}");
        }

        static async void RunRpcPullChannel()
        {
            using var client = GetTcpDmtpClient();
            var status = ChannelStatus.Default;
            var size = 0;
            var channel = client.CreateChannel();//创建通道
            var task = Task.Run(() =>//这里必须用异步
            {
                using (channel)
                {
                    foreach (var currentByteBlock in channel)
                    {
                        size += currentByteBlock.Len;//此处可以处理传递来的流数据
                    }
                    status = channel.Status;//最后状态
                }
            });
            var result = client.GetDmtpRpcActor().RpcPullChannel(channel.Id);//RpcPullChannel是代理方法，此处会阻塞至服务器全部发送完成。
            await task;//等待异步接收完成
            Console.WriteLine($"状态：{status}，size={size}");
        }

        private static async void RunRpcPushChannel()
        {
            using var client = GetTcpDmtpClient();
            var status = ChannelStatus.Default;
            var size = 0;
            var package = 1024;
            var channel = client.CreateChannel();//创建通道
            var task = Task.Run(() =>//这里必须用异步
            {
                for (var i = 0; i < 1024; i++)
                {
                    size += package;
                    channel.Write(new byte[package]);
                }
                channel.Complete();//必须调用指令函数，如Complete，Cancel，Dispose

                status = channel.Status;
            });
            var result = client.GetDmtpRpcActor().RpcPushChannel(channel.Id);//RpcPushChannel是代理方法，此处会阻塞至服务器全部完成。
            await task;//等待异步接收完成
            Console.WriteLine($"状态：{status}，result={result}");
        }


        static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
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
                    a.UseDmtpRpc();
                    //.SetSerializationSelector(new MySerializationSelector())//自定义序列化器
                    //.SetCreateDmtpRpcActor((actor) => new MyDmtpRpcActor(actor));

                    a.UseDmtpHeartbeat()
                    .SetTick(TimeSpan.FromSeconds(3))
                    .SetMaxFailCount(3);
                })
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                }));
            client.Connect();

            IRpcClient1 rpcClient1 = client.GetDmtpRpcActor<IRpcClient1>();
            IRpcClient2 rpcClient2 = client.GetDmtpRpcActor<IRpcClient2>();

            client.Logger.Info($"连接成功，Id={client.Id}");
            return client;
        }

        interface IRpcClient1 : IDmtpRpcActor
        {

        }

        interface IRpcClient2 : IDmtpRpcActor
        {

        }

        class MyDmtpRpcActor : DmtpRpcActor, IRpcClient1, IRpcClient2
        {
            public MyDmtpRpcActor(IDmtpActor smtpActor,IRpcServerProvider rpcServerProvider) : base(smtpActor, rpcServerProvider)
            {
            }
        }

        class MyClientRpcServer : RpcServer
        {
            private readonly ILog m_logger;

            public MyClientRpcServer(ILog logger)
            {
                this.m_logger = logger;
            }

            [DmtpRpc(true)]//使用函数名直接调用
            public bool Notice(string msg)
            {
                this.m_logger.Info(msg);
                return true;
            }
        }

        /// <summary>
        /// 序列化选择器
        /// </summary>
        public class MySerializationSelector : SerializationSelector
        {
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="serializationType"></param>
            /// <param name="parameterBytes"></param>
            /// <param name="parameterType"></param>
            /// <returns></returns>
            public override object DeserializeParameter(SerializationType serializationType, byte[] parameterBytes, Type parameterType)
            {
                if (parameterBytes == null)
                {
                    return parameterType.GetDefault();
                }
                switch (serializationType)
                {
                    case SerializationType.FastBinary:
                        {
                            return SerializeConvert.FastBinaryDeserialize(parameterBytes, 0, parameterType);
                        }
                    case SerializationType.SystemBinary:
                        {
                            return SerializeConvert.BinaryDeserialize(parameterBytes, 0, parameterBytes.Length);
                        }
                    case SerializationType.Json:
                        {
                            return Encoding.UTF8.GetString(parameterBytes).FromJsonString(parameterType);
                        }
                    case SerializationType.Xml:
                        {
                            return SerializeConvert.XmlDeserializeFromBytes(parameterBytes, parameterType);
                        }
                    default:
                        throw new RpcException("未指定的反序列化方式");
                }
            }

            /// <summary>
            /// 序列化参数
            /// </summary>
            /// <param name="serializationType"></param>
            /// <param name="parameter"></param>
            /// <returns></returns>
            public override byte[] SerializeParameter(SerializationType serializationType, object parameter)
            {
                if (parameter == null)
                {
                    return null;
                }
                switch (serializationType)
                {
                    case SerializationType.FastBinary:
                        {
                            return SerializeConvert.FastBinarySerialize(parameter);
                        }
                    case SerializationType.SystemBinary:
                        {
                            return SerializeConvert.BinarySerialize(parameter);
                        }
                    case SerializationType.Json:
                        {
                            return SerializeConvert.JsonSerializeToBytes(parameter);
                        }
                    case SerializationType.Xml:
                        {
                            return SerializeConvert.XmlSerializeToBytes(parameter);
                        }
                    default:
                        throw new RpcException("未指定的序列化方式");
                }
            }
        }
    }
}