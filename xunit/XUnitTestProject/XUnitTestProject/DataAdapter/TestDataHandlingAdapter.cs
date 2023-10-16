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
using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;
using Xunit.Abstractions;

namespace XUnitTestProject.DataAdapter
{
    public class TestDataHandlingAdapter
    {
        private readonly ITestOutputHelper m_output;

        public TestDataHandlingAdapter(ITestOutputHelper output)
        {
            this.m_output = output;
        }

        [Theory]
        [InlineData(10000, 1)]
        [InlineData(10000, 10)]
        [InlineData(10000, 100)]
        [InlineData(10000, 1000)]
        public void TcpDmtpAdapterShouldBeOk(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new TcpDmtpAdapter(), bufferLength, (byteBlock, requestInfo) =>
            {
              
            });//用BufferLength模拟粘包，分包

            var smtpMessage = new DmtpMessage();
            smtpMessage.ProtocolFlags = 10;
            smtpMessage.BodyByteBlock = new ByteBlock();
            smtpMessage.BodyByteBlock.Write("Dmtp");

            var data = smtpMessage.BuildAsBytes();
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 60).ToString());
        }

        [Theory]
        [InlineData(10000, 1)]
        [InlineData(10000, 10)]
        [InlineData(10000, 100)]
        [InlineData(10000, 1000)]
        public void TcpDmtpAdapterShouldBeOk_2(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new TcpDmtpAdapter(), bufferLength, (byteBlock, requestInfo) =>
            {
               
            });//用BufferLength模拟粘包，分包

            var smtpMessage = new DmtpMessage();
            smtpMessage.ProtocolFlags = 10;
            smtpMessage.BodyByteBlock = new ByteBlock();
            smtpMessage.BodyByteBlock.SetLength(1024);

            var data = smtpMessage.BuildAsBytes();
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 60).ToString());
        }

        [Theory]
        [InlineData(10000, 1)]
        [InlineData(10000, 10)]
        [InlineData(10000, 100)]
        [InlineData(10000, 1000)]
        public void FixedHeaderPackageAdapterShouldBeOk(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new FixedHeaderPackageAdapter() { }, bufferLength);//用BufferLength模拟粘包，分包

            var data = Encoding.UTF8.GetBytes("RRQM");

            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 60).ToString());
        }

        [Theory]
        [InlineData(10000, 10)]
        [InlineData(10000, 100)]
        [InlineData(10000, 1000)]
        public void FixedSizeShouldBeOk(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new FixedSizePackageAdapter(1024), bufferLength);//用BufferLength模拟粘包，分包

            var data = Encoding.UTF8.GetBytes("RRQM");

            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 60).ToString());
        }

        [Theory]
        [InlineData(10000, 10)]
        [InlineData(10000, 100)]
        [InlineData(10000, 1000)]
        public void TerminatorShouldBeOk(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new TerminatorPackageAdapter("\r\n"), bufferLength);//用BufferLength模拟粘包，分包

            var data = Encoding.UTF8.GetBytes("RRQM");

            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 60).ToString());
        }

        [Theory]
        [InlineData(1000, 1)]
        [InlineData(10000, 10)]
        [InlineData(10000, 100)]
        [InlineData(10000, 1000)]
        [InlineData(10000, 1024 * 64)]
        public void HttpServerAdapterShouldBeOk(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new HttpServerDataHandlingAdapter(), bufferLength, (byteBlock, request) =>
             {
                 if (request is HttpRequest httpRequest1)
                 {
                     httpRequest1.TryGetContent(out _);
                 }
                
             });//用BufferLength模拟粘包，分包

            var byteBlock = BytePool.Default.GetByteBlock(1024);
            var httpRequest = new HttpRequest();
            httpRequest.Method = TouchSocket.Http.HttpMethod.Get;
            httpRequest.FromText("RRQM");
            httpRequest.Build(byteBlock);
            var data = byteBlock.ToArray();
            var s = Encoding.UTF8.GetString(data);
            byteBlock.Dispose();

            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 10).ToString());
        }

        [Theory]
        [InlineData(10000, 1)]
        [InlineData(10000, 10)]
        [InlineData(10000, 100)]
        [InlineData(10000, 1000)]
        public void HttpClientAdapterShouldBeOk(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new HttpClientDataHandlingAdapter(), bufferLength, (byteBlock, request) =>
             {
                 if (request is HttpResponse httpResponse1)
                 {
                     httpResponse1.TryGetContent(out _);
                 }

                
             });//用BufferLength模拟粘包，分包

            var byteBlock = BytePool.Default.GetByteBlock(1024);
            var httpResponse = new HttpResponse();
            httpResponse.FromText("RRQM");
            httpResponse.Build(byteBlock);
            var data = byteBlock.ToArray();
            var s = Encoding.UTF8.GetString(data);
            byteBlock.Dispose();

            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 10).ToString());
        }

        [Theory]
        [InlineData(1024, 1)]
        [InlineData(1024, 10)]
        [InlineData(1024, 1024)]
        [InlineData(1024, 1024 * 64)]
        [InlineData(10240, 10)]
        [InlineData(10240, 1024)]
        [InlineData(10240, 1024 * 64)]
        [InlineData(65536, 10)]
        [InlineData(65536, 1024)]
        [InlineData(65536, 1024 * 64)]
        [InlineData(655360, 100)]
        [InlineData(655360, 1024)]
        [InlineData(655360, 1024 * 64)]
        public void WebSocketAdapterNoMaskShouldBeOk(int dataLen, int bufferLength)
        {
            var data = new byte[dataLen];
            new Random().NextBytes(data);

            var testCount = 5000;

            var dataFrame = new WSDataFrame();
            dataFrame.AppendBinary(data, 0, data.Length);
            var newdata = dataFrame.BuildResponseToBytes();

            var success = true;
            var tester = TcpDataAdapterTester.CreateTester(new WebSocketDataHandlingAdapter(), bufferLength,//用BufferLength模拟粘包，分包
            (ByteBlock byteBlock, IRequestInfo obj) =>
            {
                var revDataFram = (WSDataFrame)obj;

                if (dataFrame.FIN != revDataFram.FIN)
                {
                    success = false;
                }
                if (dataFrame.RSV1 != revDataFram.RSV1)
                {
                    success = false;
                }
                if (dataFrame.RSV2 != revDataFram.RSV2)
                {
                    success = false;
                }
                if (dataFrame.RSV3 != revDataFram.RSV3)
                {
                    success = false;
                }
                if (dataFrame.PayloadLength == revDataFram.PayloadLength)
                {
                    for (var i = 0; i < dataFrame.PayloadLength; i++)
                    {
                        if (dataFrame.PayloadData.Buffer[i] != revDataFram.PayloadData.Buffer[i])
                        {
                            success = false;
                        }
                    }
                }
                else
                {
                    success = false;
                }
               
            });

            this.m_output.WriteLine(tester.Run(newdata, testCount, testCount, dataLen * 5).ToString());
            Assert.True(success);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        [InlineData(10000000)]
        public void WSDataNoMaskShouldBeOk(int length)
        {
            var data = new byte[length];
            new Random().NextBytes(data);

            var dataFrame = new WSDataFrame();
            dataFrame.AppendBinary(data, 0, data.Length);
            var newdata = dataFrame.BuildResponseToBytes();

            WSDataFrame rev_DataFrame = null;
            var tester = TcpDataAdapterTester.CreateTester(new WebSocketDataHandlingAdapter(), 1024,//用BufferLength模拟粘包，分包
            (ByteBlock byteBlock, IRequestInfo obj) =>
              {
                  rev_DataFrame = (WSDataFrame)obj;
                  rev_DataFrame.PayloadData.SetHolding(true);
                  
              });

            tester.Run(newdata, 1, 1, 1000);//此处模拟发送

            Thread.Sleep(1000 * 2);
            Assert.NotNull(rev_DataFrame);
            Assert.Equal(dataFrame.FIN, rev_DataFrame.FIN);
            Assert.Equal(dataFrame.RSV1, rev_DataFrame.RSV1);
            Assert.Equal(dataFrame.RSV2, rev_DataFrame.RSV2);
            Assert.Equal(dataFrame.RSV3, rev_DataFrame.RSV3);
            Assert.Equal(dataFrame.Opcode, rev_DataFrame.Opcode);
            Assert.Equal(dataFrame.Mask, rev_DataFrame.Mask);
            Assert.Equal(dataFrame.PayloadLength, rev_DataFrame.PayloadLength);
            Assert.Null(rev_DataFrame.MaskingKey);

            for (var i = 0; i < dataFrame.PayloadLength; i++)
            {
                Assert.Equal(dataFrame.PayloadData.Buffer[i], rev_DataFrame.PayloadData.Buffer[i]);
            }
            rev_DataFrame.PayloadData.SetHolding(false);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        [InlineData(10000000)]
        public void WSDataMaskShouldBeOk(int length)
        {
            var data = new byte[length];
            new Random().NextBytes(data);

            var dataFrame = new WSDataFrame();
            dataFrame.AppendBinary(data, 0, data.Length);
            var newdata = dataFrame.BuildRequestToBytes();

            WSDataFrame rev_DataFrame = null;
            var tester = TcpDataAdapterTester.CreateTester(new WebSocketDataHandlingAdapter(), 1024,//用BufferLength模拟粘包，分包
            (ByteBlock byteBlock, IRequestInfo obj) =>
              {
                  rev_DataFrame = (WSDataFrame)obj;
                  rev_DataFrame.PayloadData.SetHolding(true);
                 
              }
            );

            tester.Run(newdata, 1, 1, 1000);//此处模拟发送

            Thread.Sleep(1000 * 2);
            Assert.NotNull(rev_DataFrame);
            Assert.Equal(dataFrame.FIN, rev_DataFrame.FIN);
            Assert.Equal(dataFrame.RSV1, rev_DataFrame.RSV1);
            Assert.Equal(dataFrame.RSV2, rev_DataFrame.RSV2);
            Assert.Equal(dataFrame.RSV3, rev_DataFrame.RSV3);
            Assert.Equal(dataFrame.Opcode, rev_DataFrame.Opcode);
            Assert.Equal(dataFrame.Mask, rev_DataFrame.Mask);
            Assert.Equal(dataFrame.PayloadLength, rev_DataFrame.PayloadLength);
            Assert.NotNull(rev_DataFrame.MaskingKey);

            for (var i = 0; i < dataFrame.PayloadLength; i++)
            {
                Assert.Equal(dataFrame.PayloadData.Buffer[i], rev_DataFrame.PayloadData.Buffer[i]);
            }
            rev_DataFrame.PayloadData.SetHolding(false);
        }

        [Theory]
        [InlineData(10000, 1)]
        [InlineData(10000, 3)]
        [InlineData(10000, 5)]
        [InlineData(10000, 200)]
        [InlineData(10000, 500)]
        [InlineData(10000, 1000)]
        public void MyCustomDataHandlingAdapterShouldBeOk(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new MyFixedHeaderCustomDataHandlingAdapter(), bufferLength);//用BufferLength模拟粘包，分包

            var block = new ByteBlock();
            block.Write((byte)102);//写入数据长度
            block.Write((byte)1);//写入数据类型
            block.Write((byte)1);//写入数据指令

            var buffer = new byte[100];
            new Random().NextBytes(buffer);
            block.Write(buffer);//写入数据

            var data = block.ToArray();

            //输出测试时间，用于衡量适配性能
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 2).ToString());
        }

        [Theory]
        [InlineData(1000, 1024, 1)]
        [InlineData(1000, 1024, 3)]
        [InlineData(1000, 1024, 5)]
        [InlineData(1000, 1024, 200)]
        [InlineData(1000, 1024, 500)]
        [InlineData(1000, 1024, 1000)]
        public void MyCustomBigFixedHeaderDataHandlingAdapterShouldBeOk(int inputCount, int dataSize, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new MyCustomBigFixedHeaderDataHandlingAdapter(), bufferLength,
                receivedCallBack: (byteBlock, requestInfo) =>
                 {
                    
                 });//用BufferLength模拟粘包，分包

            var block = new ByteBlock();
            block.Write(TouchSocketBitConverter.Default.GetBytes(dataSize));//写入数据长度

            var buffer = new byte[dataSize];
            new Random().NextBytes(buffer);
            block.Write(buffer);//写入数据

            var data = block.ToArray();

            //输出测试时间，用于衡量适配性能
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 10).ToString());
        }

        [Theory]
        [InlineData(1000, 1)]
        [InlineData(1000, 3)]
        [InlineData(1000, 5)]
        [InlineData(1000, 200)]
        [InlineData(1000, 500)]
        [InlineData(1000, 1000)]
        public void MyCustomBetweenAndDataHandlingAdapterShouldBeOk_1(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(
                new MyCustomBetweenAndDataHandlingAdapter("##", "**", Encoding.UTF8), bufferLength,
                receivedCallBack: (byteBlock, requestInfo) =>
                 {
                    
                 });//用BufferLength模拟粘包，分包

            var data = Encoding.UTF8.GetBytes("##RRQM Say Hello**");

            //输出测试时间，用于衡量适配性能
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 10).ToString());
        }

        [Theory]
        [InlineData(1000, 1)]
        [InlineData(1000, 3)]
        [InlineData(1000, 5)]
        [InlineData(1000, 200)]
        [InlineData(1000, 500)]
        [InlineData(1000, 1000)]
        public void MyCustomBetweenAndDataHandlingAdapterShouldBeOk_2(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(
                new MyCustomBetweenAndDataHandlingAdapter(null, "**", Encoding.UTF8), bufferLength,
                receivedCallBack: (byteBlock, requestInfo) =>
                {
                    
                });//用BufferLength模拟粘包，分包

            var data = Encoding.UTF8.GetBytes("RRQM Say Hello**");

            //输出测试时间，用于衡量适配性能
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 10).ToString());
        }

        [Theory]
        [InlineData(1000, 1)]
        [InlineData(1000, 3)]
        [InlineData(1000, 5)]
        [InlineData(1000, 200)]
        [InlineData(1000, 500)]
        [InlineData(1000, 1000)]
        public void MyCustomBetweenAndDataHandlingAdapterShouldBeOk_3(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(
                new MyCustomBetweenAndDataHandlingAdapter("##", "**", Encoding.UTF8) { MinSize = 10 }, bufferLength,
                receivedCallBack: (byteBlock, requestInfo) =>
                {
                    if (requestInfo is MyBetweenAndRequestInfo myBetween)
                    {
                        var str = Encoding.UTF8.GetString(myBetween.Data);
                    }
                    
                });//用BufferLength模拟粘包，分包

            var data = Encoding.UTF8.GetBytes("##RRQM ** Say Hello**");

            //输出测试时间，用于衡量适配性能
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 10).ToString());
        }

        [Theory]
        [InlineData(1000, 1)]
        [InlineData(1000, 3)]
        [InlineData(1000, 5)]
        [InlineData(1000, 200)]
        [InlineData(1000, 500)]
        [InlineData(1000, 1000)]
        public void MyCustomBetweenAndDataHandlingAdapterShouldBeOk_4(int inputCount, int bufferLength)
        {
            var tester = TcpDataAdapterTester.CreateTester(new MyCustomBetweenAndDataHandlingAdapter("**", "##", Encoding.UTF8) { MinSize = 5 }, bufferLength,
                receivedCallBack: (byteBlock, requestInfo) =>
                {
                    if (requestInfo is MyBetweenAndRequestInfo myBetween)
                    {
                        var str = Encoding.UTF8.GetString(myBetween.Data);
                    }
                  
                });//用BufferLength模拟粘包，分包

            var data = Encoding.UTF8.GetBytes("**12##12##");

            //输出测试时间，用于衡量适配性能
            this.m_output.WriteLine(tester.Run(data, inputCount, inputCount, 1000 * 10).ToString());
        }

        [Fact]
        public void AdapterCallbackShouldBeOk()
        {
            var adapter = new FixedHeaderPackageAdapter();

            var sendCallBack = false;
            var receivedCallBack = false;

            byte[] sentData = null;
            adapter.SendCallBack = (buffer, offset, length) =>
            {
                //此处会回调发送的最终调用。例如：此处使用固定包头，则发送的数据为4+n的封装。
                sentData = new byte[length];
                Array.Copy(buffer, offset, sentData, 0, length);

                if (length == 4 + 4)
                {
                    sendCallBack = true;
                }
            };

            adapter.ReceivedCallBack += (byteBlock, requestInfo) =>
            {
                //此处会回调接收的最终触发，例如：此处使用的固定包头，会解析4+n的数据为n。

                if (byteBlock.Len == 4)
                {
                    receivedCallBack = true;
                }
              
            };

            var data = Encoding.UTF8.GetBytes("RRQM");

            adapter.SendInput(data, 0, data.Length);//模拟输入，会在SendCallBack中输出最终要发送的数据。

            using (var block = new ByteBlock())
            {
                block.Write(sentData);
                block.Pos = 0;
                adapter.ReceivedInput(block);//模拟输出，会在ReceivedCallBack中输出最终收到的实际数据。
            }

            Assert.True(sendCallBack);
            Assert.True(receivedCallBack);
        }

        [Theory]
        [InlineData(1,1,100)]
        [InlineData(2,2,100)]
        [InlineData(2,1024,100)]
        [InlineData(5,1,1024)]
        [InlineData(5,5,1024)]
        [InlineData(10,1024,1024*1024)]
        public void UdpPackageAdapterShouldBeOk(int multiThread, int mtu,int dataLen)
        {
            var tester = UdpDataAdapterTester.CreateTester(new UdpPackageAdapter() { MTU = mtu, Timeout = 60 * 1000 }, multiThread);

            var data = new byte[dataLen];
            this.m_output.WriteLine(tester.Run(data, 100, 100, 1000 * 60).ToString());
        }

        [Fact]
        public void UdpPackageAdapterShouldBeOk123()
        {
            for (var i = 0; i < 10; i++)
            {
                var multiThread = new Random().Next(1, 10);
                var mtu = new Random().Next(1024, 1024 * 50);
                var tester = UdpDataAdapterTester.CreateTester(new UdpPackageAdapter() { MTU = mtu, Timeout = 60 * 1000 }, multiThread);//用BufferLength模拟粘包，分包

                var data = new byte[new Random().Next(1, 1024 * 1024)];
                new Random().NextBytes(data);

                this.m_output.WriteLine(tester.Run(data, 10, 10, 1000 * 60).ToString());
            }
        }
    }
}