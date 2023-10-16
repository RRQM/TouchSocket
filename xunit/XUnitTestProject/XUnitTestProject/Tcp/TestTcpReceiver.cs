using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace XUnitTestProject.Tcp
{
    public class TestTcpReceiver : UnitBase
    {
        [Fact]
        public async Task TcpClientReceiverShouleBeOk()
        {
            int waitTime = 500;
            var port = new Random().Next(10000, 60000);
            using var service = new TcpService();
            service.Received = async (c, e) =>
            {
                await c.SendAsync(e.ByteBlock);
            };
            service.Start(port);

            var received = false;
            using var client = new TcpClient();
            client.Received = (c, e) =>
            {
                received = true;
                return Task.CompletedTask;
            };
            client.Connect($"127.0.0.1:{port}");


            using (var receiver = client.CreateReceiver())
            {
                for (int i = 0; i < 10; i++)
                {
                    client.Send("RRQM");
                    await Task.Delay(waitTime);
                    using (var receiveResult = await receiver.ReadAsync(CancellationToken.None))
                    {
                        var str = receiveResult.ByteBlock.ToString();
                        Assert.Equal("RRQM", str);
                    }
                }
            }

            Assert.False(received);

            client.Send("RRQM");

            await Task.Delay(waitTime);
            Assert.True(received);
        }

        [Fact]
        public async Task TcpServiceReceiverShouleBeOk()
        {
            int waitTime = 500;
            var port = new Random().Next(10000, 60000);
            using var service = new TcpService();

            int cd = 0;
            int dcd = 0;
            int rev = 0;
            service.Connected = async (c, e) =>
            {
                cd++;
                using (var receiver = c.CreateReceiver())
                {
                    while (true)
                    {
                        using (var receiveResult = await receiver.ReadAsync(CancellationToken.None))
                        {
                            if (receiveResult.IsClosed)
                            {
                                break;
                            }
                            var str = receiveResult.ByteBlock.ToString();
                            Assert.Equal("RRQM", str);
                            rev++;
                        }
                    }
                }
                dcd++;
            };
            service.Start(port);

            using var client = new TcpClient();
            client.Connect($"127.0.0.1:{port}");
            await Task.Delay(waitTime);

            for (int i = 0; i < 10; i++)
            {
                await client.SendAsync("RRQM");
                await Task.Delay(waitTime);
            }

            Assert.True(cd==1);
            Assert.True(dcd==0);
            Assert.True(rev==10);

            client.Close();
            await Task.Delay(waitTime);
            Assert.True(cd == 1);
            Assert.True(dcd == 1);
            Assert.True(rev == 10);
        }
    }
}
