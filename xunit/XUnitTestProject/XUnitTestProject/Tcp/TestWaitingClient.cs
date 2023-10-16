using TouchSocket.Sockets;

namespace XUnitTestProject.Tcp
{
    public class TestWaitingClient : UnitBase
    {
        [Fact]
        public void WaitingClientShouldBeOk()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("tcp://127.0.0.1:7789");

            var waitingClient = tcpClient.CreateWaitingClient(new WaitingOptions()
            {
                BreakTrigger = true,
                ThrowBreakException = true
            });

            var len = Random.Shared.Next(10, 100);
            var buffer = new byte[len];
            Random.Shared.NextBytes(buffer);
            var bytes = waitingClient.SendThenReturn(buffer);
            Assert.True(bytes.SequenceEqual(bytes));
        }

        [Fact]
        public async Task WaitingClientAsyncShouldBeOk()
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("tcp://127.0.0.1:7789");

            var waitingClient = tcpClient.CreateWaitingClient(new WaitingOptions()
            {
                BreakTrigger = true,
                ThrowBreakException = true
            });

            var len = Random.Shared.Next(10, 100);
            var buffer = new byte[len];
            Random.Shared.NextBytes(buffer);
            var bytes = await waitingClient.SendThenReturnAsync(buffer);
            Assert.True(bytes.SequenceEqual(bytes));
        }

        [Fact]
        public void FilterWaitingClientShouldBeOk()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("tcp://127.0.0.1:7789");

            while (true)
            {
                var filter = false;

                var waitingClient = tcpClient.CreateWaitingClient(new WaitingOptions()
                {
                    BreakTrigger = true,
                    ThrowBreakException = true,
                    FilterFunc = (response) =>
                    {
                        if (response.Data != null)
                        {
                            if (response.Data.ToList().Contains(9))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                });

                var buffer = new byte[1024];
                Random.Shared.NextBytes(buffer);
                filter = buffer.ToList().Contains(0);
                if (filter)
                {
                    var bytes = waitingClient.SendThenReturn(buffer);
                    Assert.True(bytes.SequenceEqual(bytes));
                    return;
                }
                else
                {
                    Assert.ThrowsAny<TimeoutException>(() =>
                    {
                        var bytes = waitingClient.SendThenReturn(buffer);
                    });
                }
            }
        }

        [Fact]
        public async Task FilterWaitingClientAsyncShouldBeOk()
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("tcp://127.0.0.1:7789");

            while (true)
            {
                var filter = false;

                var waitingClient = tcpClient.CreateWaitingClient(new WaitingOptions()
                {
                    BreakTrigger = true,
                    ThrowBreakException = true,
                    FilterFunc = (response) =>
                    {
                        if (response.Data != null)
                        {
                            if (response.Data.ToList().Contains(9))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                });

                var buffer = new byte[1024];
                Random.Shared.NextBytes(buffer);
                filter = buffer.ToList().Contains(0);
                if (filter)
                {
                    var bytes = await waitingClient.SendThenReturnAsync(buffer);
                    Assert.True(bytes.SequenceEqual(bytes));
                    return;
                }
                else
                {
                    await Assert.ThrowsAnyAsync<TimeoutException>(async () =>
                     {
                         var bytes = await waitingClient.SendThenReturnAsync(buffer);
                     });
                }
            }
        }
    }
}