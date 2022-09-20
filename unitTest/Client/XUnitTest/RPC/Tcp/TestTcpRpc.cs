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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Data;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;
using TouchSocket.Core.Serialization;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.TouchRpc.Plugins;
using TouchSocket.Sockets;
using Xunit;

namespace XUnitTest.RPC.Tcp
{
    public class CallbackServer : RpcServer
    {
        public int count;

        [TouchRpc]
        public int Add(int a, int b)
        {
            Interlocked.Increment(ref count);
            return a + b;
        }

        [TouchRpc]
        public int Ref(ref int a, out int b)
        {
            b = a;
            a++;
            return a + b;
        }

        [TouchRpc]
        public string SayHello(int age)
        {
            return $"我今年{age}岁了";
        }
    }

    public class TestTcpRpc : UnitBase
    {
        [Fact]
        public void ConnectShouldBeOk()
        {
            ThreadPool.SetMinThreads(50, 50);
            List<Task> tasks = new List<Task>();
            for (int j = 0; j < 10; j++)
            {
                Task task = Task.Run(() =>
                  {
                      var client = GetClient(false);
                      bool connecting;
                      bool connected;
                      bool disConnected;
                      bool handshaked;

                      client.Connecting += (client, e) =>
                      {
                          connecting = true;
                      };
                      client.Connected += (client, e) =>
                      {
                          connected = true;
                      };
                      client.Disconnected += (client, e) =>
                      {
                          disConnected = e.Message == "主动断开" && e.Manual;
                      };
                      client.AddPlugin<TouchRpcActionPlugin<TcpTouchRpcClient>>()
                      .SetHandshaked((client, e) =>
                      {
                          handshaked = true;
                      });

                      for (int i = 0; i < 5; i++)
                      {
                          connecting = false;
                          connected = false;
                          disConnected = false;
                          handshaked = false;

                          client.Connect();
                          Assert.True(client.Online);
                          Assert.True(client.IsHandshaked);
                          Assert.True(connecting);
                          Assert.True(connected);
                          Assert.True(handshaked);

                          client.Close("主动断开");

                          Assert.True(!client.Online);
                          Assert.True(!client.IsHandshaked);
                          Assert.True(connected);
                      }
                  });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        [Fact]
        public void CreateChannelToClientShouldBeOk_1()
        {
            var client1 = GetClient();
            var client2 = GetClient();

            int id = 0;
            Task.Run(() =>
            {
                try
                {
                    using (Channel channel = client1.CreateChannel(client2.ID))
                    {
                        channel.MaxSpeed = int.MaxValue;
                        id = channel.ID;
                        byte[] buffer = new byte[1024 * 1024];
                        for (int i = 0; i < 1000; i++)
                        {
                            channel.Write(buffer);
                        }
                        channel.Complete();
                    }
                }
                catch (Exception ex)
                {
                }
            });
            Thread.Sleep(1000);

            long len = 0;
            if (client2.TrySubscribeChannel(id, out Channel channel))
            {
                channel.MaxSpeed = int.MaxValue;
                using (channel)
                {
                    while (channel.MoveNext())
                    {
                        len += channel.GetCurrent().Length;
                    }
                    Assert.Equal(1024 * 1024 * 1000, len);
                    Assert.Equal(ChannelStatus.Completed, channel.Status);
                }
            }
            else
            {
                throw new Exception();
            }
        }

        [Fact]
        public void CreateChannelToClientShouldBeOk_2()
        {
            var client1 = GetClient();
            var client2 = GetClient();

            long len = 0;
            int id = 0;

            ChannelStatus status;
            Task.Run(() =>
            {
                try
                {
                    using (Channel channel = client1.CreateChannel(client2.ID))
                    {
                        channel.MaxSpeed = int.MaxValue;
                        id = channel.ID;

                        while (channel.MoveNext())
                        {
                            len += channel.GetCurrent().Length;
                        }
                        status = channel.Status;
                    }
                }
                catch (Exception ex)
                {
                }
            });

            Thread.Sleep(1000);
            if (client2.TrySubscribeChannel(id, out Channel channel))
            {
                channel.MaxSpeed = int.MaxValue;
                using (channel)
                {
                    byte[] buffer = new byte[1024 * 1024];
                    for (int i = 0; i < 1000; i++)
                    {
                        channel.Write(buffer);
                    }
                    channel.Complete();

                    Thread.Sleep(1000);
                    Assert.Equal(1024 * 1024 * 1000, len);
                    Assert.Equal(ChannelStatus.Completed, channel.Status);
                }
            }
            else
            {
                throw new Exception();
            }
        }

        [Fact]
        public void CreateChannelToServerShouldBeOk()
        {
            var client = GetClient();

            byte[] data = new byte[0];
            short protocol = -1;
            client.AddPlugin<TouchRpcActionPlugin<TcpTouchRpcClient>>()
                .SetReceivedProtocolData((protocolClient, e) =>
                {
                    data = e.ByteBlock.ToArray(2);
                    protocol = e.Protocol;
                });
            using (Channel channel = client.CreateChannel())
            {
                channel.MaxSpeed = int.MaxValue;
                client.Send(10000, TouchSocketBitConverter.Default.GetBytes(channel.ID));
                byte[] buffer = new byte[1024 * 1024];
                for (int i = 0; i < 1000; i++)
                {
                    channel.Write(buffer);
                }
                channel.Complete();

                Thread.Sleep(1000 * 5);
                Assert.Equal(10000, protocol);
            }
        }

        [Theory]
        [InlineData(SerializationType.FastBinary)]
        [InlineData(SerializationType.Json)]
        [InlineData(SerializationType.Xml)]
        public void InvokeClientByIDShouldBeOk(SerializationType serializationType)
        {
            var client1 = GetClient();
            var client2 = GetClient();

            RpcStore rpcService = new RpcStore(new Container());
            rpcService.AddRpcParser("client1", client1);
            rpcService.AddRpcParser("client2", client2);
            rpcService.RegisterServer<CallbackServer>();

            InvokeOption invokeOption = new InvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 1000
            };

            var t1 = Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    int sum = client1.Invoke<int>(client1.ID, "xunittest.rpc.tcp.callbackserver.add", invokeOption, 10, 20);
                    Assert.Equal(30, sum);

                    int sum2 = client1.Invoke<int>(client2.ID, "xunittest.rpc.tcp.callbackserver.add", invokeOption, 10, 20);
                    Assert.Equal(30, sum2);
                }
            });

            var t2 = Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    int sum = client2.Invoke<int>(client2.ID, "xunittest.rpc.tcp.callbackserver.add", invokeOption, 10, 20);
                    Assert.Equal(30, sum);

                    int sum2 = client2.Invoke<int>(client1.ID, "xunittest.rpc.tcp.callbackserver.add", invokeOption, 10, 20);
                    Assert.Equal(30, sum2);
                }
            });

            Task.WaitAll(new Task[] { t1, t2 });
        }

        [Theory]
        [InlineData(SerializationType.FastBinary)]
        [InlineData(SerializationType.Json)]
        [InlineData(SerializationType.Xml)]
        public void InvokeRefClientByIDShouldBeOk(SerializationType serializationType)
        {
            var client1 = GetClient();
            var client2 = GetClient();
            RpcStore rpcService = new RpcStore(new Container());
            rpcService.AddRpcParser("client1", client1);
            rpcService.AddRpcParser("client2", client2);
            rpcService.RegisterServer<CallbackServer>();

            InvokeOption invokeOption = new InvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 1000
            };

            Type[] types = new Type[] { typeof(int), typeof(int) };
            var t1 = Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    int a = 10;
                    int b = 0;
                    object[] ps = new object[] { a, b };
                    int sum = client1.Invoke<int>(client2.ID, "xunittest.rpc.tcp.callbackserver.ref", invokeOption, ref ps, types);
                    Assert.Equal(21, sum);
                    Assert.Equal(11, ps[0]);
                    Assert.Equal(10, ps[1]);
                }
            });

            var t2 = Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    int a = 10;
                    int b = 0;
                    object[] ps = new object[] { a, b };
                    int sum = client2.Invoke<int>(client1.ID, "xunittest.rpc.tcp.callbackserver.ref", invokeOption, ref ps, types);
                    Assert.Equal(21, sum);
                    Assert.Equal(11, ps[0]);
                    Assert.Equal(10, ps[1]);
                }
            });

            Task.WaitAll(new Task[] { t1, t2 });
        }

        [Theory]
        [InlineData(SerializationType.FastBinary)]
        [InlineData(SerializationType.Json)]
        [InlineData(SerializationType.Xml)]
        public void InvokeServiceShouldBeOk(SerializationType serializationType)
        {
            var client = GetClient();

            InvokeOption invokeOption = new InvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 1000
            };

            RemoteTest remoteTest = new RemoteTest(client);
            remoteTest.Test01(invokeOption);
            remoteTest.Test02(invokeOption);
            remoteTest.Test03(invokeOption);
            remoteTest.Test04(invokeOption);
            remoteTest.Test05(invokeOption);
            remoteTest.Test06(invokeOption);
            remoteTest.Test07(invokeOption);
            remoteTest.Test08(invokeOption);
            remoteTest.Test09(invokeOption);
            remoteTest.Test10(invokeOption);
            remoteTest.Test11(invokeOption);

            if (serializationType != SerializationType.Xml)
            {
                remoteTest.Test12(invokeOption);//xml不支持序列化字典
            }

            remoteTest.Test13(invokeOption);
            remoteTest.Test14(invokeOption);
            remoteTest.Test15(invokeOption);
            remoteTest.Test16(invokeOption);
            remoteTest.Test17(invokeOption);
            remoteTest.Test18(invokeOption);
            //remoteTest.Test19(client.ID);
            remoteTest.Test22(invokeOption);
            remoteTest.Test25(invokeOption);
        }

        [Fact]
        public void PingShouldBeOk()
        {
            var client = GetClient();
            Assert.True(!string.IsNullOrEmpty(client.ID));

            for (int i = 0; i < 1000; i++)
            {
                Assert.True(client.Ping());
            }
        }

        [Fact]
        public void NewPingShouldBeOk()
        {
            var client1 = GetClient();
            client1.ResetID("1");
            var client2 = GetClient();
            client2.ResetID("2");
            Assert.True(!string.IsNullOrEmpty(client1.ID));

            for (int i = 0; i < 1000; i++)
            {
                //Assert.True(client1.Ping(".", client2.ID));
            }
        }

        [Fact]
        public void ReceiveStreamFromServerShouldBeOk()
        {
            var client = GetClient();

            bool streamTransfering = false;
            bool streamTransfered = false;
            bool streamTransferSuccess = false;
            client.AddPlugin<TouchRpcActionPlugin<TcpTouchRpcClient>>()
                .SetStreamTransfering((protocolClient, e) =>
                {
                    e.Bucket = new MemoryStream();
                    streamTransfering = true;
                })
                .SetStreamTransfered((protocolClient, e) =>
                {
                    if (e.Bucket.Length == 1024 * 1024 * 100 && e.Metadata["1"] == "1" && e.Result.ResultCode == ResultCode.Success)
                    {
                        streamTransferSuccess = true;
                    }
                    streamTransfered = true;
                });

            client.Send(20001);
            SpinWait.SpinUntil(() => streamTransfered, 1000 * 20);

            Assert.True(streamTransfering);
            Assert.True(streamTransfered);
            Assert.True(streamTransferSuccess);
        }

        [Fact]
        public void ResetIDShouldBeOk()
        {
            var client = GetClient();
            Assert.True(!string.IsNullOrEmpty(client.ID));

            string id = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                id += i;
                client.ResetID(id);
                Assert.Equal(id, client.ID);
            }
            id = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                id += i;
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    client.Send(10001, byteBlock.Write(id));
                }
                Thread.Sleep(50);
                Assert.Equal(id, client.ID);
            }
        }

        [Theory]
        [InlineData(SerializationType.FastBinary)]
        [InlineData(SerializationType.Json)]
        [InlineData(SerializationType.Xml)]
        public void ReverseInvokeServiceShouldBeOk(SerializationType serializationType)
        {
            var client = GetClient();
            client.RegisterServer<CallbackServer>();

            InvokeOption invokeOption = new InvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 1000
            };

            RemoteTest remoteTest = new RemoteTest(client);

            remoteTest.Test19(client.ID, invokeOption);
        }

        [Fact]
        public void SendAndReceiveShouldBeOk()
        {
            var client = GetClient();

            int receivedCount = 0;
            byte[] data = new byte[0];
            short protocol = -1;

            client.AddPlugin<TouchRpcActionPlugin<TcpTouchRpcClient>>()
                .SetReceivedProtocolData((protocolClient, e) =>
                {
                    receivedCount++;
                    data = e.ByteBlock.ToArray(2);
                    protocol = e.Protocol;
                });

            for (short i = 1; i < 100; i++)
            {
                client.Send(i, TouchSocketBitConverter.Default.GetBytes(i));
                Thread.Sleep(20);

                Assert.Equal(i, protocol);
                Assert.True(data.Length == 2);
                Assert.Equal(i, TouchSocketBitConverter.Default.ToInt16(data, 0));
            }
        }

        [Fact]
        public void SendStreamToServerShouldBeOk()
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception)
            {
            }
            var client = GetClient();

            byte[] data = new byte[0];
            short protocol = -1;
            client.AddPlugin<TouchRpcActionPlugin<TcpTouchRpcClient>>()
                 .SetReceivedProtocolData((protocolClient, e) =>
                 {
                     data = e.ByteBlock.ToArray(2);
                     protocol = e.Protocol;
                 });

            using (MemoryStream stream = new MemoryStream(new byte[1024 * 1024 * 100]))
            {
                StreamOperator streamOperator = new StreamOperator();
                streamOperator.SetMaxSpeed(int.MaxValue);
                Metadata metadata = new Metadata();
                metadata.Add("1", "1");
                Result result = client.SendStream(stream, streamOperator, metadata);

                Assert.True(result.ResultCode == ResultCode.Success);
                Assert.Equal(result.ResultCode, streamOperator.Result.ResultCode);
            }

            Thread.Sleep(1000);

            Assert.Equal(20000, protocol);
        }

        [Fact]
        public void CancellationTokenInvokeShouldBeOk()
        {
            var client = GetClient();
            RemoteTest remoteTest = new RemoteTest(client);
            remoteTest.Test26();
        }

        private TcpTouchRpcClient GetClient(bool con = true)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();

            client.Setup(new TouchSocketConfig()
                .UsePlugin()
                .SetThreadCount(20)
                .SetHeartbeatFrequency(null)
                .SetRemoteIPHost(new IPHost("127.0.0.1:7794"))
                .SetVerifyToken("123RPC"));
            if (con)
            {
                client.Connect();
            }
            return client;
        }

        [Fact]
        public void RemoteStreamShouldBeOk()
        {
            var client = GetClient();
            RemoteStream remoteStream = client.LoadRemoteStream(new Metadata().Add("1", "1"));

            byte[] data = new byte[] { 0, 1, 2, 3, 4 };
            remoteStream.Write(data);
            Assert.True(remoteStream.Length == 5);
            Assert.True(remoteStream.Position == 5);

            remoteStream.Position = 0;
            byte[] buffer = new byte[5];
            remoteStream.Read(buffer);
            Assert.True(buffer.SequenceEqual(data));
            Assert.True(remoteStream.Position == 5);
            Assert.True(remoteStream.Length == 5);

            remoteStream.SafeDispose();

            Assert.ThrowsAny<Exception>(() =>
            {
                remoteStream.Flush();
            });
        }
        [Fact]
        public void CompressRemoteStreamShouldBeOk()
        {
            var client = GetClient();
            RemoteStream remoteStream = client.LoadRemoteStream(new Metadata().Add("2", "2"));
            remoteStream.DataCompressor = new GZipDataCompressor();

            byte[] data = new byte[] { 0, 1, 2, 3, 4 };
            remoteStream.Write(data);
            Assert.True(remoteStream.Length == 5);
            Assert.True(remoteStream.Position == 5);

            remoteStream.Position = 0;
            byte[] buffer = new byte[5];
            remoteStream.Read(buffer);
            Assert.True(buffer.SequenceEqual(data));
            Assert.True(remoteStream.Position == 5);
            Assert.True(remoteStream.Length == 5);

            remoteStream.SafeDispose();

            Assert.ThrowsAny<Exception>(() =>
            {
                remoteStream.Flush();
            });
        }

        [Fact]
        public void RemoteAccessShouldBeOk()
        {
            var client = GetClient();
            string dirPath = Path.GetFullPath("Test");

            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            for (int i = 0; i < 10; i++)
            {
                File.WriteAllBytes(Path.Combine(dirPath, $"{i}.txt"), new byte[1024]);
                Directory.CreateDirectory(Path.Combine(dirPath, i.ToString()));
            }

            RemoteDirectoryInfoResult rootDirectoryInfoResult = client.GetDirectoryInfo(dirPath, new Metadata(), 5000, new CancellationToken());
            Assert.True(rootDirectoryInfoResult.ResultCode == ResultCode.Success);
            Assert.NotNull(rootDirectoryInfoResult.DirectoryInfo);
            Assert.True(rootDirectoryInfoResult.DirectoryInfo.Directories.Length == 10);
            Assert.True(rootDirectoryInfoResult.DirectoryInfo.Files.Length == 10);

            for (int i = 0; i < 10; i++)
            {
                var directoryInfoResult = client.GetDirectoryInfo(Path.Combine(dirPath,
                    rootDirectoryInfoResult.DirectoryInfo.Directories[i]));
                Assert.True(directoryInfoResult.ResultCode == ResultCode.Success);
                Assert.NotNull(directoryInfoResult.DirectoryInfo);
                Assert.True(directoryInfoResult.DirectoryInfo.Directories.Length == 0);
                Assert.True(directoryInfoResult.DirectoryInfo.Files.Length == 0);

                RemoteFileInfoResult fileInfoResult = client.GetFileInfo(Path.Combine(dirPath, rootDirectoryInfoResult.DirectoryInfo.Files[i]));
                Assert.True(fileInfoResult.ResultCode == ResultCode.Success);
                Assert.True(fileInfoResult.FileInfo.FileLength == 1024);
            }


        }
    }

    public class TestTcpRpc2
    {
        [Fact]
        public void CreateChannelToServerShouldBeOk()
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception)
            {
            }
            var client = GetClient();
            Assert.True(!string.IsNullOrEmpty(client.ID));
            var t1 = Task.Run(() =>
              {
                  byte[] data = new byte[0];
                  short protocol = -1;
                  client.AddPlugin<TouchRpcActionPlugin<TcpTouchRpcClient>>()
                 .SetReceivedProtocolData((protocolClient, e) =>
                 {
                     data = e.ByteBlock.ToArray(2);
                     protocol = e.Protocol;
                 });

                  using (Channel channel = client.CreateChannel())
                  {
                      channel.MaxSpeed = 1024 * 1024;
                      client.Send(10000, TouchSocketBitConverter.Default.GetBytes(channel.ID));
                      byte[] buffer = new byte[1024 * 1024];
                      for (int i = 0; i < 1000 * 5; i++)
                      {
                          channel.Write(buffer);
                      }
                      channel.Complete();

                      Thread.Sleep(1000);
                      Assert.Equal(10000, protocol);
                  }
              });

            var t2 = Task.Run(() =>
              {
                  for (int i = 0; i < 100000; i++)
                  {
                      Assert.True(client.Ping());
                  }
              });
            Task.WaitAll(new Task[] { t1, t2 });
        }

        private TcpTouchRpcClient GetClient(bool con = true)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7794"))
                .SetVerifyToken("123RPC"));
            if (con)
            {
                client.Connect();
            }
            return client;
        }
    }
}