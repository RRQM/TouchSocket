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

            var waitingClient = tcpClient.CreateWaitingClient(new WaitingOptions());

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

            var waitingClient = tcpClient.CreateWaitingClient(new WaitingOptions());

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