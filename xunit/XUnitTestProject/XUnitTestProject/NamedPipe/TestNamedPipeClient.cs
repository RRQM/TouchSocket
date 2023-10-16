using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace XUnitTestProject.NamedPipe
{
    public class TestNamedPipeClient : UnitBase
    {
        [Fact]
        public void ConnectShouldBeOk()
        {
            var waitTime = 100;
            var client = new NamedPipeClient();

            var connectingCount = 0L;
            var connectedCount = 0L;
            var disconnectedCount = 0L;
            var disconnectingCount = 0L;

            client.Connecting = (client, e) =>
            {
                Interlocked.Increment(ref connectingCount);
                return Task.CompletedTask;
            };

            client.Connected = (client, e) =>
            {
                Interlocked.Increment(ref connectedCount);
                return Task.CompletedTask;
            };

            client.Disconnecting = (client, e) =>
            {
                Interlocked.Increment(ref disconnectingCount);
                return Task.CompletedTask;
            };

            client.Disconnected = (client, e) =>
            {
                Interlocked.Increment(ref disconnectedCount);
                return Task.CompletedTask;
            };

            var receivedCount = 0;
            client.Received = (tcpClient, e) =>
            {
                receivedCount++;
                return Task.CompletedTask;
            };

            client.Setup(new TouchSocket.Core.TouchSocketConfig()
                .SetPipeName("pipe7789"));//载入配置

            for (var i = 0; i < 10; i++)
            {
                client.Connect();
                Assert.True(client.Online);
                Thread.Sleep(waitTime);

                Assert.Equal(i + 1, Interlocked.Read(ref connectingCount));
                Assert.Equal(i + 1, Interlocked.Read(ref connectedCount));

                client.Close();
                Thread.Sleep(waitTime);

                Assert.False(client.Online);
                //Assert.Equal(i + 1, Interlocked.Read(ref disconnectingCount));
                Assert.Equal(i + 1, Interlocked.Read(ref disconnectedCount));
            }

            client.Connect();
            Thread.Sleep(waitTime);

            connectingCount = 0;
            connectedCount = 0;
            disconnectingCount = 0;
            disconnectedCount = 0;

            client.Send(BitConverter.GetBytes(1));
            Thread.Sleep(waitTime);
            Assert.Equal(1, receivedCount);

            client.Send(BitConverter.GetBytes(2));
            Thread.Sleep(waitTime);
            Assert.Equal(2, receivedCount);

            client.Dispose();
            Assert.False(client.Online);

            Thread.Sleep(waitTime);
            Assert.Equal(1, Interlocked.Read(ref disconnectingCount));
            Assert.Equal(1, Interlocked.Read(ref disconnectedCount));

            Assert.ThrowsAny<Exception>(() =>
            {
                client.Connect();
            });

            connectingCount = 0;
            connectedCount = 0;
            disconnectingCount = 0;
            disconnectedCount = 0;

            Assert.Equal(0, Interlocked.Read(ref connectingCount));
            Assert.Equal(0, Interlocked.Read(ref connectedCount));
            Assert.Equal(0, Interlocked.Read(ref disconnectingCount));
            Assert.Equal(0, Interlocked.Read(ref disconnectedCount));
        }

        [Fact]
        public async Task ConnectAsyncShouldBeOk()
        {
            var waitTime = 100;
            var client = new NamedPipeClient();

            var connectingCount = 0L;
            var connectedCount = 0L;
            var disconnectedCount = 0L;
            var disconnectingCount = 0L;

            client.Connecting = (client, e) =>
            {
                Interlocked.Increment(ref connectingCount);
                return Task.CompletedTask;
            };

            client.Connected = (client, e) =>
            {
                Interlocked.Increment(ref connectedCount);
                return Task.CompletedTask;
            };

            client.Disconnecting = (client, e) =>
            {
                Interlocked.Increment(ref disconnectingCount);
                return Task.CompletedTask;
            };

            client.Disconnected = (client, e) =>
            {
                Interlocked.Increment(ref disconnectedCount);
                return Task.CompletedTask;
            };

            var receivedCount = 0;
            client.Received = (c,e) =>
            {
                receivedCount++;
                return Task.CompletedTask;
            };

            client.Setup(new TouchSocket.Core.TouchSocketConfig()
                .SetPipeName("pipe7789"));//载入配置

            for (var i = 0; i < 10; i++)
            {
                await client.ConnectAsync();

                Assert.True(client.Online);
                await Task.Delay(waitTime);

                Assert.Equal(i + 1, Interlocked.Read(ref connectingCount));
                Assert.Equal(i + 1, Interlocked.Read(ref connectedCount));

                client.Close();
                await Task.Delay(waitTime);

                Assert.False(client.Online);
                //Assert.Equal(i + 1, Interlocked.Read(ref disconnectingCount));
                Assert.Equal(i + 1, Interlocked.Read(ref disconnectedCount));
            }

            await client.ConnectAsync();
            await Task.Delay(waitTime);

            connectingCount = 0;
            connectedCount = 0;
            disconnectingCount = 0;
            disconnectedCount = 0;

            client.Send(BitConverter.GetBytes(1));
            await Task.Delay(waitTime);
            Assert.Equal(1, receivedCount);

            client.Send(BitConverter.GetBytes(2));
            await Task.Delay(waitTime);
            Assert.Equal(2, receivedCount);

            client.Dispose();
            Assert.False(client.Online);

            await Task.Delay(waitTime);
            Assert.Equal(1, Interlocked.Read(ref disconnectingCount));
            Assert.Equal(1, Interlocked.Read(ref disconnectedCount));

            await Assert.ThrowsAnyAsync<Exception>(async () =>
             {
                 await client.ConnectAsync();
             });

            connectingCount = 0;
            connectedCount = 0;
            disconnectingCount = 0;
            disconnectedCount = 0;

            Assert.Equal(0, Interlocked.Read(ref connectingCount));
            Assert.Equal(0, Interlocked.Read(ref connectedCount));
            Assert.Equal(0, Interlocked.Read(ref disconnectingCount));
            Assert.Equal(0, Interlocked.Read(ref disconnectedCount));
        }
    }
}