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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AdapterConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var service = await CreateService();
            var client = await CreateClient();

            ConsoleLogger.Default.Info("输入任意内容，回车发送（将会循环发送10次）");
            while (true)
            {
                var str = Console.ReadLine();
                for (var i = 0; i < 10; i++)
                {
                    await client.SendAsync(str);
                }
            }
        }

        private static async Task<TcpClient> CreateClient()
        {
            var client = new TcpClient();
            //载入配置
            await client.SetupAsync(new TouchSocketConfig()
                 .SetRemoteIPHost("127.0.0.1:7789")
                 .SetTcpDataHandlingAdapter(() => new MyDataHandleAdapter())
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个日志注入
                 }));

            await client.ConnectAsync();//调用连接，当连接不成功时，会抛出异常。
            client.Logger.Info("客户端成功连接");
            return client;
        }

        private static async Task<TcpService> CreateService()
        {
            var service = new TcpService();
            service.Received = (client, e) =>
            {
                //从客户端收到信息
                var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
                return Task.CompletedTask;
            };

            await service.SetupAsync(new TouchSocketConfig()//载入配置
                 .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                 .SetTcpDataHandlingAdapter(() => new PeriodPackageAdapter() { CacheTimeout=TimeSpan.FromSeconds(1) })
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                 })
                 .ConfigurePlugins(a =>
                 {
                     //a.Add();//此处可以添加插件
                 }));
            await service.StartAsync();//启动
            service.Logger.Info("服务器已启动");
            return service;
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

        protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
        {
            //收到从原始流式数据。

            var buffer = byteBlock.TotalMemory.GetArray().Array;
            var r = byteBlock.Length;
            if (this.m_tempByteBlock == null)//如果没有临时包，则直接分包。
            {
                await this.SplitPackageAsync(buffer, 0, r);
            }
            else
            {
                if (this.m_surPlusLength == r)//接收长度正好等于剩余长度，组合完数据以后直接处理数据。
                {
                    this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(buffer, 0, this.m_surPlusLength));
                    await this.PreviewHandleAsync(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    this.m_surPlusLength = 0;
                }
                else if (this.m_surPlusLength < r)//接收长度大于剩余长度，先组合包，然后处理包，然后将剩下的分包。
                {
                    this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(buffer, 0, this.m_surPlusLength));
                    await this.PreviewHandleAsync(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    await this.SplitPackageAsync(buffer, this.m_surPlusLength, r);
                }
                else//接收长度小于剩余长度，无法处理包，所以必须先组合包，然后等下次接收。
                {
                    this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(buffer, 0, r));
                    this.m_surPlusLength -= (byte)r;
                }
            }
        }


        protected override async Task PreviewSendAsync(ReadOnlyMemory<byte> memory)
        {
            //在发送流式数据之前

            var length = memory.Length;
            if (length > byte.MaxValue)//超长判断
            {
                throw new OverlengthException("发送数据太长。");
            }

            //从内存池申请内存块，因为此处数据绝不超过255，所以避免内存池碎片化，每次申请1K
            //ByteBlock byteBlock = new ByteBlock(dataLen+1);//实际写法。
            using (var byteBlock = new ByteBlock(1024))
            {
                byteBlock.WriteByte((byte)length);//先写长度
                byteBlock.Write(memory.Span);//再写数据
                await this.GoSendAsync(byteBlock.Memory);
            }
        }

        protected override async Task PreviewSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            //使用拼接模式发送，在发送流式数据之前

            var dataLen = 0;
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
                byteBlock.WriteByte((byte)dataLen);//先写长度
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));//依次写入
                }
                await this.GoSendAsync(byteBlock.Memory);
            }
        }

        protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
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
                    byteBlock.WriteByte((byte)data.Length);//先写长度
                    byteBlock.WriteByte((byte)myClass.DataType);//然后数据类型
                    byteBlock.WriteByte((byte)myClass.OrderType);//然后指令类型
                    byteBlock.Write(data);//再写数据
                    await this.GoSendAsync(byteBlock.Memory);
                }
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        private async Task PreviewHandleAsync(ByteBlock byteBlock)
        {
            try
            {
                await this.GoReceivedAsync(byteBlock, null);
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
        private async Task SplitPackageAsync(byte[] dataBuffer, int index, int r)
        {
            while (index < r)
            {
                var length = dataBuffer[index];
                var recedSurPlusLength = r - index - 1;
                if (recedSurPlusLength >= length)
                {
                    var byteBlock = new ByteBlock(length);
                    byteBlock.Write(new ReadOnlySpan<byte>(dataBuffer, index + 1, length));
                    await this.PreviewHandleAsync(byteBlock);
                    this.m_surPlusLength = 0;
                }
                else//半包
                {
                    this.m_tempByteBlock = new ByteBlock(length);
                    this.m_surPlusLength = (byte)(length - recedSurPlusLength);
                    this.m_tempByteBlock.Write(new ReadOnlySpan<byte>(dataBuffer, index + 1, recedSurPlusLength));
                }
                index += length + 1;
            }
        }
    }

    internal class MyClass : IRequestInfo
    {
        public OrderType OrderType { get; set; }
        public DataType DataType { get; set; }

        public byte[] Data { get; set; }
    }

    internal enum DataType : byte
    {
        Down = 0,
        Up = 1
    }

    internal enum OrderType : byte
    {
        Hold = 0,
        Go = 1
    }
}