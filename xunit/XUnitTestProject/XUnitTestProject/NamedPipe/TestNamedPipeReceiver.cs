using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;
using Xunit.Abstractions;

namespace XUnitTestProject.NamedPipe
{
    public class TestNamedPipeReceiver: UnitBaseOutput
    {
        public TestNamedPipeReceiver(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task NamedPipeClientReceiverShouleBeOk()
        {
            int waitTime = 500;
            var named = Guid.NewGuid().ToString();
            using var service = new NamedPipeService();
            service.Received = async (c, e) =>
            {
                await c.SendAsync(e.ByteBlock);
            };
            service.Start(named);

            var received = false;
            using var client = new NamedPipeClient();
            client.Received = (c, e) =>
            {
                received = true;
                return Task.CompletedTask;
            };
            client.Connect(named);

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
        public async Task NamedPipeServiceReceiverShouleBeOk()
        {
            int waitTime = 500;
            var named = Guid.NewGuid().ToString();
            using var service = new NamedPipeService();

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
                            this.OutputHelper.WriteLine("rev="+str);
                            rev++;
                        }
                    }
                }
                dcd++;
            };
            service.Start(named);
            this.OutputHelper.WriteLine("start");


            using var client = new NamedPipeClient();
            client.Connect(named);
            this.OutputHelper.WriteLine("connected");

            for (int i = 0; i < 10; i++)
            {
                await client.SendAsync("RRQM");
                await Task.Delay(waitTime);
                this.OutputHelper.WriteLine("send"+i);
            }

            Assert.True(cd == 1);
            Assert.True(dcd == 0);
            Assert.True(rev == 10);

            client.Close();
            await Task.Delay(waitTime);
            Assert.True(cd == 1);
            Assert.True(dcd == 1);
            Assert.True(rev == 10);
        }
    }
}
