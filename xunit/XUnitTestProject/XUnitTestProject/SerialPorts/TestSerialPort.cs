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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace XUnitTestProject.SerialPorts
{
    public class TestSerialPort
    {
        public TestSerialPort()
        {
            this.CreateClient();
        }
        SerialPortClient m_serialPortClient;
        private void CreateClient()
        {
            var serialPortClient = new SerialPortClient();
            serialPortClient.Setup(new TouchSocket.Core.TouchSocketConfig()
                .SetSerialPortOption(this.GetOption("COM1")));

            serialPortClient.Received = async (c, e) =>
            {
                await c.SendAsync(e.ByteBlock.ToArray());
            };

            serialPortClient.Connect();
            this.m_serialPortClient = serialPortClient;
        }

        private SerialPortOption GetOption(string portName)
        {
            return new SerialPortOption()
            {
                BaudRate = 115200,
                DataBits = 8,
                Parity = System.IO.Ports.Parity.None,
                PortName = portName,
                StopBits = System.IO.Ports.StopBits.One
            };
        }

        [Fact]
        public async Task SendThenReturnShouldBeOk()
        {
            var waitTime = 1000;
            var client = new SerialPortClient();
            client.Setup(new TouchSocketConfig()
                .SetSerialPortOption(GetOption("COM2")));//载入配置
            await client.ConnectAsync();//连接

            var waitingClient = client.CreateWaitingClient(new WaitingOptions());

            var data = new byte[] { 0, 1, 2, 3, 4 };
            var returnData = waitingClient.SendThenReturn(data);
            Assert.True(returnData.SequenceEqual(data));

            var returnData2 = await waitingClient.SendThenReturnAsync(data);
            Assert.True(returnData2.SequenceEqual(data));
        }

        [Fact]
        public async Task ShouldCanConnectAndReceive()
        {
            var waitTime = 1000;
            var client = new SerialPortClient();

            var connected = false;
            var disconnectCount = 0;
            client.Connected = async (client, e) =>
            {
                connected = true;
                await Task.CompletedTask;
            };
            client.Disconnected = async (client, e) =>
            {
                disconnectCount++;
                connected = false;
                await Task.CompletedTask;
            };

            var receivedCount = 0;
            client.Received = async (tcpClient, e) =>
            {
                receivedCount++;
                await Task.CompletedTask;
            };

            client.Setup(new TouchSocketConfig()
                .SetSerialPortOption(GetOption("COM2")));//载入配置
            await client.ConnectAsync();//连接

            await Task.Delay(waitTime);

            Assert.True(client.Online);
            Assert.True(connected);

            client.Send(BitConverter.GetBytes(1));
            await Task.Delay(waitTime);
            Assert.Equal(1, receivedCount);

            client.Send(BitConverter.GetBytes(2));
            await Task.Delay(waitTime);
            Assert.Equal(2, receivedCount);

            client.Close();
            await Task.Delay(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(1, disconnectCount);

            client.Connect();
            await Task.Delay(waitTime);
            Assert.True(client.Online);
            Assert.True(connected);

            client.Send(BitConverter.GetBytes(3));
            await Task.Delay(waitTime);
            Assert.Equal(3, receivedCount);

            client.Send(BitConverter.GetBytes(4));
            await Task.Delay(waitTime);
            Assert.Equal(4, receivedCount);

            client.Dispose();
            await Task.Delay(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(2, disconnectCount);

            Assert.ThrowsAny<Exception>(() =>
            {
                client.Connect();
            });

            await Task.Delay(waitTime);
            Assert.Equal(2, disconnectCount);
        }
    }
}