using System;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AdapterConsoleApp
{

    internal class Program
    {
        private static TcpClient CreateClient()
        {
            var client = new TcpClient();
            //载入配置
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetTcpDataHandlingAdapter(() => new MyDataHandleAdapter())
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
            service.Received = (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                var mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);//注意：数据长度是byteBlock.Len
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .SetTcpDataHandlingAdapter(() => new MyDataHandleAdapter())
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                }))
                .Start();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }

        private static void Main(string[] args)
        {
            var service = CreateService();
            var client = CreateClient();

            ConsoleLogger.Default.Info("输入任意内容，回车发送（将会循环发送10次）");
            while (true)
            {
                var str = Console.ReadLine();
                for (var i = 0; i < 10; i++)
                {
                    client.Send(str);
                }
            }
        }
    }

    internal class MyDataHandleAdapter : SingleStreamDataHandlingAdapter
    {
        /// <summary>
        /// 包剩余长度
        /// </summary>
        private byte m_surPlusLength;

        /// <summary>
        /// 临时包，此包仅当前实例储存
        /// </summary>
        private ByteBlock m_tempByteBlock;

        public override bool CanSendRequestInfo => false;

        public override bool CanSplicingSend => false;

        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            //收到从原始流式数据。

            var buffer = byteBlock.Buffer;
            var r = byteBlock.Len;
            if (this.m_tempByteBlock == null)//如果没有临时包，则直接分包。
            {
                this.SplitPackage(buffer, 0, r);
            }
            else
            {
                if (this.m_surPlusLength == r)//接收长度正好等于剩余长度，组合完数据以后直接处理数据。
                {
                    this.m_tempByteBlock.Write(buffer, 0, this.m_surPlusLength);
                    this.PreviewHandle(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    this.m_surPlusLength = 0;
                }
                else if (this.m_surPlusLength < r)//接收长度大于剩余长度，先组合包，然后处理包，然后将剩下的分包。
                {
                    this.m_tempByteBlock.Write(buffer, 0, this.m_surPlusLength);
                    this.PreviewHandle(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    this.SplitPackage(buffer, this.m_surPlusLength, r);
                }
                else//接收长度小于剩余长度，无法处理包，所以必须先组合包，然后等下次接收。
                {
                    this.m_tempByteBlock.Write(buffer, 0, r);
                    this.m_surPlusLength -= (byte)r;
                }
            }
        }

        protected override void PreviewSend(byte[] buffer, int offset, int length)
        {
            //在发送流式数据之前

            if (length > byte.MaxValue)//超长判断
            {
                throw new OverlengthException("发送数据太长。");
            }

            //从内存池申请内存块，因为此处数据绝不超过255，所以避免内存池碎片化，每次申请1K
            //ByteBlock byteBlock = new ByteBlock(dataLen+1);//实际写法。
            using (var byteBlock = new ByteBlock(1024))
            {
                byteBlock.Write((byte)length);//先写长度
                byteBlock.Write(buffer, offset, length);//再写数据
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        {
            //使用拼接模式发送，在发送流式数据之前

            int dataLen = 0;
            foreach (var item in transferBytes)
            {
                dataLen += item.Count;
            }
            if (dataLen > byte.MaxValue)//超长判断
            {
                throw new OverlengthException("发送数据太长。");
            }

            //从内存池申请内存块，因为此处数据绝不超过255，所以避免内存池碎片化，每次申请1K
            //ByteBlock byteBlock = new ByteBlock(dataLen+1);//实际写法。

            using (var byteBlock = new ByteBlock(1024))
            {
                byteBlock.Write((byte)dataLen);//先写长度
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(item.Array, item.Offset, item.Count);//依次写入
                }
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            //使用对象发送，在发送流式数据之前

            if (requestInfo is MyClass myClass)
            {
                var data = myClass.Data ?? Array.Empty<byte>();
                if (data.Length > byte.MaxValue)//超长判断
                {
                    throw new OverlengthException("发送数据太长。");
                }

                //从内存池申请内存块，因为此处数据绝不超过255，所以避免内存池碎片化，每次申请1K
                //ByteBlock byteBlock = new ByteBlock(dataLen+1);//实际写法。
                using (var byteBlock = new ByteBlock(1024))
                {
                    byteBlock.Write((byte)data.Length);//先写长度
                    byteBlock.Write((byte)myClass.DataType);//然后数据类型
                    byteBlock.Write((byte)myClass.OrderType);//然后指令类型
                    byteBlock.Write(data);//再写数据
                    this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        private void PreviewHandle(ByteBlock byteBlock)
        {
            try
            {
                this.GoReceived(byteBlock, null);
            }
            finally
            {
                byteBlock.Dispose();//在框架里面将内存块释放
            }
        }

        /// <summary>
        /// 分解包
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="index"></param>
        /// <param name="r"></param>
        private void SplitPackage(byte[] dataBuffer, int index, int r)
        {
            while (index < r)
            {
                var length = dataBuffer[index];
                var recedSurPlusLength = r - index - 1;
                if (recedSurPlusLength >= length)
                {
                    var byteBlock = new ByteBlock(length);
                    byteBlock.Write(dataBuffer, index + 1, length);
                    this.PreviewHandle(byteBlock);
                    this.m_surPlusLength = 0;
                }
                else//半包
                {
                    this.m_tempByteBlock = new ByteBlock(length);
                    this.m_surPlusLength = (byte)(length - recedSurPlusLength);
                    this.m_tempByteBlock.Write(dataBuffer, index + 1, recedSurPlusLength);
                }
                index += length + 1;
            }
        }
    }

    class MyClass : IRequestInfo
    {
        public OrderType OrderType { get; set; }
        public DataType DataType { get; set; }

        public byte[] Data { get; set; }
    }

    enum DataType : byte
    {
        Down = 0,
        Up = 1
    }

    enum OrderType : byte
    {
        Hold = 0,
        Go = 1
    }
}