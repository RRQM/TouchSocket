using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CustomAdapterConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = CreateService();
            var client = CreateClient();

            ConsoleLogger.Default.Info("按任意键发送10次");
            while (true)
            {
                Console.ReadKey();
                for (var i = 0; i < 10; i++)
                {
                    var myRequestInfo = new MyRequestInfo()
                    {
                        Body = Encoding.UTF8.GetBytes("hello"),
                        DataType = (byte)i,
                        OrderType = (byte)i
                    };

                    //构建发送数据
                    using (var byteBlock = new ByteBlock(1024))
                    {
                        byteBlock.Write((byte)(myRequestInfo.Body.Length + 2));//先写长度，因为该长度还包含数据类型和指令类型，所以+2
                        byteBlock.Write((byte)myRequestInfo.DataType);//然后数据类型
                        byteBlock.Write((byte)myRequestInfo.OrderType);//然后指令类型
                        byteBlock.Write(myRequestInfo.Body);//再写数据
                        client.Send(byteBlock);
                    }
                }
            }
        }

        private static TcpClient CreateClient()
        {
            var client = new TcpClient();
            //载入配置
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetTcpDataHandlingAdapter(() => new MyCustomDataHandlingAdapter())
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));

            client.Connect();//调用连接，当连接不成功时，会抛出异常。
            client.Logger.Info("客户端成功连接");
            return client;
        }

        private static TcpService CreateService()
        {
            var service = new TcpService();
            service.Received = (client, e) =>
            {
                //从客户端收到信息

                if (e.RequestInfo is MyRequestInfo myRequest)
                {
                    client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Body)}");
                }
                return Task.CompletedTask;
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .SetTcpDataHandlingAdapter(() => new MyCustomDataHandlingAdapter())
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                }));
            service.Start();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }

    internal class MyCustomDataHandlingAdapter : CustomDataHandlingAdapter<MyRequestInfo>
    {
        /// <summary>
        /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
        /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
        /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
        /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
        /// </summary>
        /// <param name="byteBlock">字节块</param>
        /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
        /// <param name="request">对象。</param>
        /// <param name="tempCapacity">缓存容量指导，指示当需要缓存时，应该申请多大的内存。</param>
        /// <returns></returns>
        protected override FilterResult Filter(in ByteBlock byteBlock, bool beCached, ref MyRequestInfo request, ref int tempCapacity)
        {
            //以下解析思路为一次性解析，不考虑缓存的临时对象。

            if (byteBlock.CanReadLen < 3)
            {
                return FilterResult.Cache;//当头部都无法解析时，直接缓存
            }

            var pos = byteBlock.Pos;//记录初始游标位置，防止本次无法解析时，回退游标。

            var myRequestInfo = new MyRequestInfo();

            //此操作实际上有两个作用，
            //1.填充header
            //2.将byteBlock.Pos递增3的长度。
            byteBlock.Read(out var header, 3);//填充header

            //因为第一个字节表示所有长度，而DataType、OrderType已经包含在了header里面。
            //所有只需呀再读取header[0]-2个长度即可。
            var bodyLength = (byte)(header[0] - 2);

            if (bodyLength > byteBlock.CanReadLen)
            {
                //body数据不足。
                byteBlock.Pos = pos;//回退游标
                return FilterResult.Cache;
            }
            else
            {
                //此操作实际上有两个作用，
                //1.填充body
                //2.将byteBlock.Pos递增bodyLength的长度。
                byteBlock.Read(out var body, bodyLength);

                myRequestInfo.DataType = header[1];
                myRequestInfo.OrderType = header[2];
                myRequestInfo.Body = body;
                request = myRequestInfo;//赋值ref
                return FilterResult.Success;//返回成功
            }
        }
    }

    internal class MyRequestInfo : IRequestInfo
    {
        /// <summary>
        /// 自定义属性,Body
        /// </summary>
        public byte[] Body { get; internal set; }

        /// <summary>
        /// 自定义属性,DataType
        /// </summary>
        public byte DataType { get; internal set; }

        /// <summary>
        /// 自定义属性,OrderType
        /// </summary>
        public byte OrderType { get; internal set; }
    }
}