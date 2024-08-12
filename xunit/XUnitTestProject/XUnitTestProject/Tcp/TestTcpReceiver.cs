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