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
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.Redis;
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Dmtp.RemoteStream;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using XUnitTestProject.Rpc;

namespace XUnitTestProject.Dmtp
{
    public class TestWebSocketDmtpClient : UnitBase
    {
        [Fact]
        public async Task CancellationTokenInvokeShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());
            var remoteTest = new RemoteTest(client.GetDmtpRpcActor());
            remoteTest.Test26();
        }

        [Fact]
        public async Task CompressRemoteStreamShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());
            var remoteStream = client.GetDmtpRemoteStreamActor().LoadRemoteStream(new Metadata().Add("2", "2"));
            remoteStream.DataCompressor = new GZipDataCompressor();

            var data = new byte[] { 0, 1, 2, 3, 4 };
            remoteStream.Write(data);
            Assert.True(remoteStream.Length == 5);
            Assert.True(remoteStream.Position == 5);

            remoteStream.Position = 0;
            var buffer = new byte[5];
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
        public async Task  ConnectShouldBeOk()
        {
            var tasks = new List<Task>();
            for (var j = 0; j < 10; j++)
            {
                var task = Task.Run(async () =>
                  {
                      var client = await this.GetClient(this.GetConfig(), false);
                      var disConnected = 0;
                      var handshaked = 0;

                      client.Disconnected = (client, e) =>
                      {
                          disConnected += 1;
                          return Task.CompletedTask;
                      };

                      client.PluginsManager.Add(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), () =>
                      {
                          handshaked += 1;
                      });

                      for (var i = 0; i < 10; i++)
                      {
                          disConnected = 0;
                          handshaked = 0;

                          await client.ConnectAsync();
                          Assert.True(client.IsHandshaked);
                          Assert.Equal(1, handshaked);

                          client.Close("主动断开");

                          await Task.Delay(100);
                          Assert.False(client.IsHandshaked);
                          Assert.Equal(1, disConnected);
                      }
                  });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
        }

        [Fact]
        public async Task CreateChannelToClientShouldBeOk_1()
        {
            var client1 = await this.GetClient(this.GetConfig());
            var client2 = await this.GetClient(this.GetConfig());
            client2.PluginsManager.Add(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), (CreateChannelEventArgs e) =>
            {
            });
            var id = 0;
            _ = Task.Run(() =>
            {
                try
                {
                    using (var channel = client1.CreateChannel(client2.Id))
                    {
                        channel.MaxSpeed = int.MaxValue;
                        id = channel.Id;
                        var buffer = new byte[1024 * 1024];
                        for (var i = 0; i < 1000; i++)
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
            if (client2.TrySubscribeChannel(id, out var channel))
            {
                channel.MaxSpeed = int.MaxValue;
                using (channel)
                {
                    //while (channel.MoveNext())//旧版
                    //{
                    //    using (var byteBlock=channel.GetCurrent())
                    //    {
                    //        len += byteBlock.Len;
                    //    }
                    //}

                    foreach (var byteBlock in channel)//新版
                    {
                        using (byteBlock)
                        {
                            len += byteBlock.Len;
                        }
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
        public async Task CreateChannelToClientShouldBeOk_2()
        {
            var client1 = await this.GetClient(this.GetConfig());
            var client2 = await this.GetClient(this.GetConfig());

            long len = 0;
            var id = 0;

            ChannelStatus status;
            _ = Task.Run(() =>
            {
                try
                {
                    using (var channel = client1.CreateChannel(client2.Id))
                    {
                        channel.MaxSpeed = int.MaxValue;
                        id = channel.Id;
                        foreach (var byteBlock in channel)
                        {
                            using (byteBlock)
                            {
                                len += channel.GetCurrent().Len;
                            }
                        }
                        status = channel.Status;
                    }
                }
                catch (Exception ex)
                {
                }
            });

            Thread.Sleep(2000);
            if (client2.TrySubscribeChannel(id, out var channel))
            {
                channel.MaxSpeed = int.MaxValue;
                using (channel)
                {
                    var buffer = new byte[1024 * 1024];
                    for (var i = 0; i < 1000; i++)
                    {
                        channel.Write(buffer);
                    }
                    channel.Complete();

                    Thread.Sleep(2000);
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
        public async Task CreateChannelToServerShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());

            var data = new byte[0];
            ushort protocol = 0;

            client.PluginsManager.Add(nameof(IDmtpReceivedPlugin.OnDmtpReceived), (DmtpMessageEventArgs e) =>
            {
                data = e.DmtpMessage.BodyByteBlock.ToArray();
                protocol = e.DmtpMessage.ProtocolFlags;
            });

            using (var channel = client.CreateChannel(new Metadata()
            {
                { "target","server"}
            }))
            {
                channel.MaxSpeed = int.MaxValue;
                var buffer = new byte[1024 * 1024];
                for (var i = 0; i < 1000; i++)
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
        public async void InvokeClientByIDShouldBeOk(SerializationType serializationType)
        {
            var config = this.GetConfig();

            var client1 = await this.GetClient(config);
            var client2 = await this.GetClient(config);

            var invokeOption = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 1000
            };

            for (var i = 0; i < 1000; i++)
            {
                var sum = client1.GetDmtpRpcActor().InvokeT<int>(client1.Id, "Add", invokeOption, 10, 20);
                Assert.Equal(30, sum);

                var sum2 = client1.GetDmtpRpcActor().InvokeT<int>(client2.Id, "Add", invokeOption, 10, 20);
                Assert.Equal(30, sum2);
            }
        }

        [Theory]
        [InlineData(SerializationType.FastBinary)]
        [InlineData(SerializationType.Json)]
        [InlineData(SerializationType.Xml)]
        public async Task InvokeRefClientByIDShouldBeOk(SerializationType serializationType)
        {
            var config = this.GetConfig();

            var client1 =await this.GetClient(config);
            var client2 =await this.GetClient(config);

            var invokeOption = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 5000
            };

            var types = new Type[] { typeof(int), typeof(int) };
            var t1 = Task.Run(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var a = 10;
                    var b = 0;
                    var ps = new object[] { a, b };
                    var sum = client1.GetDmtpRpcActor().InvokeT<int>(client2.Id, "Ref", invokeOption, ref ps, types);
                    Assert.Equal(21, sum);
                    Assert.Equal(11, ps[0]);
                    Assert.Equal(10, ps[1]);
                }
            });

            var t2 = Task.Run(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var a = 10;
                    var b = 0;
                    var ps = new object[] { a, b };
                    var sum = client2.GetDmtpRpcActor().InvokeT<int>(client1.Id, "Ref", invokeOption, ref ps, types);
                    Assert.Equal(21, sum);
                    Assert.Equal(11, ps[0]);
                    Assert.Equal(10, ps[1]);
                }
            });

            await Task.WhenAll(new Task[] { t1, t2 });
        }

        [Theory]
        [InlineData(SerializationType.FastBinary)]
        [InlineData(SerializationType.Json)]
        [InlineData(SerializationType.Xml)]
        public async void InvokeServiceShouldBeOk(SerializationType serializationType)
        {
            var client = await this.GetClient(this.GetConfig());

            var invokeOption = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 1000
            };

            var remoteTest = new RemoteTest(client.GetDmtpRpcActor());
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
        public async Task Ping2CShouldBeOk()
        {
            var client1 = await this.GetClient(this.GetConfig());
            client1.ResetId("111111111");
            var client2 = await this.GetClient(this.GetConfig());
            client2.ResetId("222222222");
            Assert.True(!string.IsNullOrEmpty(client1.Id));

            for (var i = 0; i < 1000; i++)
            {
                Assert.True(client1.Ping(client2.Id));
            }
        }

        [Fact]
        public async Task PingAsyncShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());
            Assert.True(!string.IsNullOrEmpty(client.Id));

            for (var i = 0; i < 1000; i++)
            {
                Assert.True(await client.PingAsync());
            }
        }

        [Fact]
        public async Task PingSelfShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());
            Assert.True(!string.IsNullOrEmpty(client.Id));

            for (var i = 0; i < 1000; i++)
            {
                Assert.True(client.Ping(client.Id));
            }
        }

        [Fact]
        public async Task PingShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());
            Assert.True(!string.IsNullOrEmpty(client.Id));

            for (var i = 0; i < 100000; i++)
            {
                Assert.True(client.Ping());
            }
        }

        [Fact]
        public async Task PullFile2CShouldBeOk()
        {
            for (var j = 0; j < 10; j++)
            {
                var client1 = await this.GetClient(this.GetConfig());
                var client2 = await this.GetClient(this.GetConfig());

                var path = this.GetType().Name + "PullFile2C.test";
                var savePath = this.GetType().Name + "SavePullFile2C.test";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                using (var writer = FilePool.GetWriter(path))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var buffer = new byte[1024 * 1024];
                        new Random().NextBytes(buffer);
                        //byte[] buffer = new byte[] {0,1,2,3,4,5,6,7,8,9 };
                        writer.Write(buffer);
                    }
                }

                var result = client2.GetDmtpFileTransferActor().PullFile(client1.Id, new FileOperator() { ResourcePath = Path.GetFullPath(path), SavePath = savePath });

                Assert.True(result.IsSuccess());
                Assert.True(File.Exists(savePath));

                using (var pathReader = FilePool.GetReader(path))
                {
                    using (var savePathReader = FilePool.GetReader(savePath))
                    {
                        Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                        var buffer1 = new byte[1024 * 1024];
                        var buffer2 = new byte[1024 * 1024];

                        while (true)
                        {
                            var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                            var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                            Assert.True(r1 == r2);
                            if (r1 == 0)
                            {
                                break;
                            }
                            for (var i = 0; i < r1; i++)
                            {
                                Assert.Equal(buffer1[i], buffer2[i]);
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task PullFileShouldBeOk()
        {
            for (var j = 0; j < 10; j++)
            {
                var client = await this.GetClient(this.GetConfig());
                var path = this.GetType().Name + "PullFile.test";
                var savePath = this.GetType().Name + "SavePullFile.test";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                using (var writer = FilePool.GetWriter(path))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var buffer = new byte[1024 * 1024];
                        new Random().NextBytes(buffer);
                        //byte[] buffer = new byte[] {0,1,2,3,4,5,6,7,8,9 };
                        writer.Write(buffer);
                    }
                }

                var fileOperator = new MultithreadingFileOperator()
                {
                    ResourcePath = Path.GetFullPath(path),
                    SavePath = savePath,
                    MultithreadingCount = 1
                };

                var result = client.GetDmtpFileTransferActor().PullFile(fileOperator);

                Assert.True(result.IsSuccess());
                Assert.True(File.Exists(savePath));

                using (var pathReader = FilePool.GetReader(path))
                {
                    using (var savePathReader = FilePool.GetReader(savePath))
                    {
                        Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                        var buffer1 = new byte[1024 * 1024];
                        var buffer2 = new byte[1024 * 1024];

                        while (true)
                        {
                            var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                            var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                            Assert.True(r1 == r2);
                            if (r1 == 0)
                            {
                                break;
                            }
                            for (var i = 0; i < r1; i++)
                            {
                                Assert.Equal(buffer1[i], buffer2[i]);
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task PullSmallFile2C_ShouldBeOk()
        {
            var path = this.GetType().Name + "PullSmallFile2C.test";
            var savePath = Path.GetFullPath(this.GetType().Name + "SavePullSmallFile2C.test");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using (var writer = FilePool.GetWriter(path))
            {
                var buffer = new byte[new Random().Next(1024, 1024 * 1024)];
                new Random().NextBytes(buffer);
                writer.Write(buffer);
            }

            var client1 = await this.GetClient(this.GetConfig());
            var client2 = await this.GetClient(this.GetConfig());

            var result = client1.GetDmtpFileTransferActor().PullSmallFile(client2.Id, Path.GetFullPath(path), default, 10000);
            Assert.True(result.IsSuccess());

            result.Save(savePath);
            using (var pathReader = FilePool.GetReader(path))
            {
                using (var savePathReader = FilePool.GetReader(savePath))
                {
                    Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                    var buffer1 = new byte[1024 * 1024];
                    var buffer2 = new byte[1024 * 1024];

                    while (true)
                    {
                        var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                        var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                        Assert.True(r1 == r2);
                        if (r1 == 0)
                        {
                            break;
                        }
                        for (var i = 0; i < r1; i++)
                        {
                            Assert.Equal(buffer1[i], buffer2[i]);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task PullSmallFileShouldBeOk()
        {
            var path = this.GetType().Name + "PullSmallFile.test";
            var savePath = Path.GetFullPath(this.GetType().Name + "SavePullSmallFile.test");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using (var writer = FilePool.GetWriter(path))
            {
                var buffer = new byte[new Random().Next(1024, 1024 * 1024)];
                new Random().NextBytes(buffer);
                writer.Write(buffer);
            }

            var client = await this.GetClient(this.GetConfig());
            var result = client.GetDmtpFileTransferActor().PullSmallFile(Path.GetFullPath(path), default, 10000);
            Assert.True(result.IsSuccess());

            result.Save(savePath);
            using (var pathReader = FilePool.GetReader(path))
            {
                using (var savePathReader = FilePool.GetReader(savePath))
                {
                    Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                    var buffer1 = new byte[1024 * 1024];
                    var buffer2 = new byte[1024 * 1024];

                    while (true)
                    {
                        var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                        var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                        Assert.True(r1 == r2);
                        if (r1 == 0)
                        {
                            break;
                        }
                        for (var i = 0; i < r1; i++)
                        {
                            Assert.Equal(buffer1[i], buffer2[i]);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task PushFile2CShouldBeOk()
        {
            for (var j = 0; j < 10; j++)
            {
                var client1 = await this.GetClient(this.GetConfig());
                var client2 = await this.GetClient(this.GetConfig());

                var path = this.GetType().Name + "PushFile2C.test";
                var savePath = "SavePushFile2C.test";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                using (var writer = FilePool.GetWriter(path))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var buffer = new byte[1024 * 1024];
                        new Random().NextBytes(buffer);
                        //byte[] buffer = new byte[] {0,1,2,3,4,5,6,7,8,9 };
                        writer.Write(buffer);
                    }
                }

                var result = client2.GetDmtpFileTransferActor().PushFile(client1.Id, new FileOperator() { ResourcePath = path, SavePath = Path.GetFullPath(savePath) });

                Assert.True(result.IsSuccess());
                Assert.True(File.Exists(savePath));

                using (var pathReader = FilePool.GetReader(path))
                {
                    using (var savePathReader = FilePool.GetReader(savePath))
                    {
                        Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                        var buffer1 = new byte[1024 * 1024];
                        var buffer2 = new byte[1024 * 1024];

                        while (true)
                        {
                            var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                            var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                            Assert.True(r1 == r2);
                            if (r1 == 0)
                            {
                                break;
                            }
                            for (var i = 0; i < r1; i++)
                            {
                                Assert.Equal(buffer1[i], buffer2[i]);
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task PushFileShouldBeOk()
        {
            for (var j = 0; j < 10; j++)
            {
                var client = await this.GetClient(this.GetConfig());
                var path = this.GetType().Name + "PushFile.test";
                var savePath = "SavePushFile.test";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                using (var writer = FilePool.GetWriter(path))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var buffer = new byte[1024 * 1024];
                        new Random().NextBytes(buffer);
                        //byte[] buffer = new byte[] {0,1,2,3,4,5,6,7,8,9 };
                        writer.Write(buffer);
                    }
                }

                var result = client.GetDmtpFileTransferActor().PushFile(new FileOperator() { ResourcePath = path, SavePath = Path.GetFullPath(savePath) });

                Assert.True(result.IsSuccess());
                Assert.True(File.Exists(savePath));

                using (var pathReader = FilePool.GetReader(path))
                {
                    using (var savePathReader = FilePool.GetReader(savePath))
                    {
                        Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                        var buffer1 = new byte[1024 * 1024];
                        var buffer2 = new byte[1024 * 1024];

                        while (true)
                        {
                            var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                            var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                            Assert.True(r1 == r2);
                            if (r1 == 0)
                            {
                                break;
                            }
                            for (var i = 0; i < r1; i++)
                            {
                                Assert.Equal(buffer1[i], buffer2[i]);
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task PushSmallFile2C_ShouldBeOk()
        {
            var path = this.GetType().Name + "PushSmallFile2C.test";
            var savePath = Path.GetFullPath(this.GetType().Name + "SavePushSmallFile2C.test");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using (var writer = FilePool.GetWriter(path))
            {
                var buffer = new byte[new Random().Next(1024, 1024 * 1024)];
                new Random().NextBytes(buffer);
                writer.Write(buffer);
            }

            var client1 = await this.GetClient(this.GetConfig());
            var client2 = await this.GetClient(this.GetConfig());
            var result = client1.GetDmtpFileTransferActor().PushSmallFile(client2.Id, Path.GetFullPath(savePath), new FileInfo(path), default, timeout: 10000);
            Assert.True(result.IsSuccess());

            using (var pathReader = FilePool.GetReader(path))
            {
                using (var savePathReader = FilePool.GetReader(savePath))
                {
                    Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                    var buffer1 = new byte[1024 * 1024];
                    var buffer2 = new byte[1024 * 1024];

                    while (true)
                    {
                        var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                        var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                        Assert.True(r1 == r2);
                        if (r1 == 0)
                        {
                            break;
                        }
                        for (var i = 0; i < r1; i++)
                        {
                            Assert.Equal(buffer1[i], buffer2[i]);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task PushSmallFileShouldBeOk()
        {
            var path = this.GetType().Name + "PushSmallFile.test";
            var savePath = Path.GetFullPath(this.GetType().Name + "SavePushSmallFile.test");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using (var writer = FilePool.GetWriter(path))
            {
                var buffer = new byte[new Random().Next(1024, 1024 * 1024)];
                new Random().NextBytes(buffer);
                writer.Write(buffer);
            }

            var client = await this.GetClient(this.GetConfig());
            var result = client.GetDmtpFileTransferActor().PushSmallFile(Path.GetFullPath(savePath), new FileInfo(path), default, timeout: 1000000);
            Assert.True(result.IsSuccess());

            using (var pathReader = FilePool.GetReader(path))
            {
                using (var savePathReader = FilePool.GetReader(savePath))
                {
                    Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                    var buffer1 = new byte[1024 * 1024];
                    var buffer2 = new byte[1024 * 1024];

                    while (true)
                    {
                        var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                        var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                        Assert.True(r1 == r2);
                        if (r1 == 0)
                        {
                            break;
                        }
                        for (var i = 0; i < r1; i++)
                        {
                            Assert.Equal(buffer1[i], buffer2[i]);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task RedisShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());
            var redis = client.GetDmtpRedisActor();

            redis.ClearCache();

            var result = redis.Set("1", "1");
            Assert.True(result);
            Assert.True(redis.ContainsCache("1"));
            var result1 = redis.Get<string>("1");
            Assert.True(result1 == "1");
            result = redis.Add("1", "11");
            Assert.False(result);
            result1 = redis.Get<string>("1");
            Assert.True(result1 == "1");

            result = redis.RemoveCache("1");
            Assert.True(result);
            Assert.False(redis.ContainsCache("1"));
            result1 = redis.Get<string>("1");
            Assert.True(result1 == null);

            var waitResult = new WaitResult()
            {
                Message = "2",
                Sign = 2,
                Status = 2
            };
            result = redis.Add("2", waitResult);
            Assert.True(result);
            Assert.True(redis.ContainsCache("2"));
            var result2 = redis.Get<WaitResult>("2");
            Assert.Equal(result2.Message, waitResult.Message);
            Assert.Equal(result2.Sign, waitResult.Sign);
            Assert.Equal(result2.Status, waitResult.Status);

            waitResult = new WaitResult()
            {
                Message = "22",
                Sign = 22,
                Status = 22
            };
            result = redis.Set("2", waitResult);
            Assert.True(result);
            Assert.True(redis.ContainsCache("2"));
            result2 = redis.Get<WaitResult>("2");
            Assert.Equal(result2.Message, waitResult.Message);
            Assert.Equal(result2.Sign, waitResult.Sign);
            Assert.Equal(result2.Status, waitResult.Status);

            for (var i = 0; i < 10; i++)
            {
                var buffer = new byte[1024 * 1024];
                new Random().NextBytes(buffer);
                result = redis.Set("3", buffer);
                Assert.True(result);
                var reBytes = redis.Get<byte[]>("3");
                Assert.NotNull(reBytes);
                Assert.True(buffer.SequenceEqual(reBytes));
            }

            redis.ClearCache();
            Assert.False(redis.ContainsCache("1"));
            Assert.False(redis.ContainsCache("2"));
            Assert.False(redis.ContainsCache("3"));
        }

        [Fact]
        public async Task RemoteAccessShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());
            var dirPath = Path.GetFullPath("Test");

            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            for (var i = 0; i < 10; i++)
            {
                File.WriteAllBytes(Path.Combine(dirPath, $"{i}.txt"), new byte[1024]);
                Directory.CreateDirectory(Path.Combine(dirPath, i.ToString()));
            }

            var rootDirectoryInfoResult = client.GetRemoteAccessActor().GetDirectoryInfo(dirPath, new Metadata(), 5000, new CancellationToken());
            Assert.True(rootDirectoryInfoResult.ResultCode == ResultCode.Success);
            Assert.NotNull(rootDirectoryInfoResult.DirectoryInfo);
            Assert.True(rootDirectoryInfoResult.DirectoryInfo.Directories.Length == 10);
            Assert.True(rootDirectoryInfoResult.DirectoryInfo.Files.Length == 10);

            for (var i = 0; i < 10; i++)
            {
                var directoryInfoResult = client.GetRemoteAccessActor().GetDirectoryInfo(Path.Combine(dirPath,
                    rootDirectoryInfoResult.DirectoryInfo.Directories[i]));
                Assert.True(directoryInfoResult.ResultCode == ResultCode.Success);
                Assert.NotNull(directoryInfoResult.DirectoryInfo);
                Assert.True(directoryInfoResult.DirectoryInfo.Directories.Length == 0);
                Assert.True(directoryInfoResult.DirectoryInfo.Files.Length == 0);

                var fileInfoResult = client.GetRemoteAccessActor().GetFileInfo(Path.Combine(dirPath, rootDirectoryInfoResult.DirectoryInfo.Files[i]));
                Assert.True(fileInfoResult.ResultCode == ResultCode.Success);
                Assert.True(fileInfoResult.FileInfo.Length == 1024);
            }
        }

        [Fact]
        public async Task RemoteStream2CShouldBeOk()
        {
            var client1 = await this.GetClient(this.GetConfig());
            var client2 = await this.GetClient(this.GetConfig());

            var memoryStream = new MemoryStream();
            client2.PluginsManager.Add(nameof(IDmtpRemoteStreamPlugin.OnLoadingStream), async (LoadingStreamEventArgs e) =>
            {
                e.IsPermitOperation = true;
                await e.WaitingLoadStreamAsync(memoryStream, TimeSpan.FromSeconds(10));
                memoryStream.Dispose();
            });

            var remoteStream = client1.GetDmtpRemoteStreamActor().LoadRemoteStream(client2.Id, new Metadata().Add("1", "1"));

            var data = new byte[] { 0, 1, 2, 3, 4 };
            remoteStream.Write(data);
            Assert.True(remoteStream.Length == 5);
            Assert.True(remoteStream.Position == 5);

            Assert.True(memoryStream.Length == 5);
            Assert.True(memoryStream.Position == 5);

            remoteStream.Position = 0;
            var buffer = new byte[5];
            remoteStream.Read(buffer);
            Assert.True(buffer.SequenceEqual(data));
            Assert.True(buffer.SequenceEqual(memoryStream.ToArray()));
            Assert.True(remoteStream.Position == 5);
            Assert.True(remoteStream.Length == 5);

            remoteStream.SafeDispose();

            Assert.ThrowsAny<Exception>(() =>
            {
                remoteStream.Flush();
            });
        }

        [Fact]
        public async Task RemoteStreamShouldBeOk()
        {
            var client = await this.GetClient(this.GetConfig());

            var remoteStream = client.GetDmtpRemoteStreamActor().LoadRemoteStream(new Metadata().Add("1", "1"));

            var data = new byte[] { 0, 1, 2, 3, 4 };
            remoteStream.Write(data);
            Assert.True(remoteStream.Length == 5);
            Assert.True(remoteStream.Position == 5);

            remoteStream.Position = 0;
            var buffer = new byte[5];
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
        public async Task RemoteStreamShouldBeOk_2()
        {
            var client = await this.GetClient(this.GetConfig());

            Assert.ThrowsAny<Exception>(() =>
            {
                var remoteStream = client.GetDmtpRemoteStreamActor().LoadRemoteStream();
            });
        }

        [Fact]
        public async Task RequestPushFileResource2CShouldBeOk()
        {
            var path = this.GetType().Name + "RequestPushFileResource2C.test";
            var savePath = Path.GetFullPath(this.GetType().Name + "SaveRequestPushFileResource2C.test");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using (var writer = FilePool.GetWriter(path))
            {
                for (var i = 0; i < 100; i++)
                {
                    var buffer = new byte[1024 * 1024];
                    new Random().NextBytes(buffer);
                    //byte[] buffer = new byte[] {0,1,2,3,4,5,6,7,8,9 };
                    writer.Write(buffer);
                }
            }

            var fileResourceLocator = new FileResourceLocator(new FileResourceInfo(new FileInfo(path), 1024 * 512));

            var client1 = await this.GetClient(this.GetConfig());
            var client2 = await this.GetClient(this.GetConfig());
            var result = client1.GetDmtpFileTransferActor().PushFileResourceInfo(client2.Id, savePath, fileResourceLocator, timeout: 1000000);

            Assert.True(result.IsSuccess());
            var t1 = Task.Run(() =>
            {
                for (var j = 0; j < fileResourceLocator.FileResourceInfo.FileSections.Length; j++)
                {
                    while (true)
                    {
                        var result = client1.GetDmtpFileTransferActor().PushFileSection(client2.Id, fileResourceLocator, fileResourceLocator.FileResourceInfo.FileSections[j]);
                        if (result.IsSuccess())
                        {
                            break;
                        }
                    }
                }
            });

            await Task.WhenAll(t1);
            var res = fileResourceLocator.TryFinished();
            Assert.True(res.IsSuccess());

            var fresult = client1.GetDmtpFileTransferActor().FinishedFileResourceInfo(client2.Id, fileResourceLocator.FileResourceInfo, ResultCode.Success);
            Assert.True(fresult.IsSuccess());
            using (var pathReader = FilePool.GetReader(path))
            {
                using (var savePathReader = FilePool.GetReader(savePath))
                {
                    Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                    var buffer1 = new byte[1024 * 1024];
                    var buffer2 = new byte[1024 * 1024];

                    while (true)
                    {
                        var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                        var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                        Assert.True(r1 == r2);
                        if (r1 == 0)
                        {
                            break;
                        }
                        for (var i = 0; i < r1; i++)
                        {
                            Assert.Equal(buffer1[i], buffer2[i]);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task RequestPushFileResourceShouldBeOk()
        {
            var path = this.GetType().Name + "RequestPushFileResource.test";
            var savePath = Path.GetFullPath(this.GetType().Name + "SaveRequestPushFileResource.test");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using (var writer = FilePool.GetWriter(path))
            {
                for (var i = 0; i < 100; i++)
                {
                    var buffer = new byte[1024 * 1024];
                    new Random().NextBytes(buffer);
                    //byte[] buffer = new byte[] {0,1,2,3,4,5,6,7,8,9 };
                    writer.Write(buffer);
                }
            }

            var fileResourceLocator = new FileResourceLocator(new FileResourceInfo(new FileInfo(path), 1024 * 512));

            var client = await this.GetClient(this.GetConfig());
            var result = client.GetDmtpFileTransferActor().PushFileResourceInfo(savePath, fileResourceLocator, timeout: 1000000);

            Assert.True(result.IsSuccess());
            var t1 = Task.Run(() =>
            {
                for (var j = 0; j < fileResourceLocator.FileResourceInfo.FileSections.Length; j++)
                {
                    while (true)
                    {
                        var result = client.GetDmtpFileTransferActor().PushFileSection(fileResourceLocator, fileResourceLocator.FileResourceInfo.FileSections[j]);
                        if (result.IsSuccess())
                        {
                            break;
                        }
                    }
                }
            });

            await Task.WhenAll(t1);
            var res = fileResourceLocator.TryFinished();
            Assert.True(res.IsSuccess());

            var fresult = client.GetDmtpFileTransferActor().FinishedFileResourceInfo(fileResourceLocator.FileResourceInfo, ResultCode.Success);
            Assert.True(fresult.IsSuccess());
            using (var pathReader = FilePool.GetReader(path))
            {
                using (var savePathReader = FilePool.GetReader(savePath))
                {
                    Assert.True(pathReader.FileStorage.Length == savePathReader.FileStorage.Length);
                    var buffer1 = new byte[1024 * 1024];
                    var buffer2 = new byte[1024 * 1024];

                    while (true)
                    {
                        var r1 = pathReader.Read(buffer1, 0, buffer1.Length);
                        var r2 = savePathReader.Read(buffer2, 0, buffer2.Length);
                        Assert.True(r1 == r2);
                        if (r1 == 0)
                        {
                            break;
                        }
                        for (var i = 0; i < r1; i++)
                        {
                            Assert.Equal(buffer1[i], buffer2[i]);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task ResetIdShouldBeOk()
        {
            var client1 = await this.GetClient(this.GetConfig());
            var client2 = await this.GetClient(this.GetConfig());

            Assert.True(!string.IsNullOrEmpty(client1.Id));
            Assert.True(!string.IsNullOrEmpty(client2.Id));

            var id = string.Empty;
            for (var i = 0; i < 10; i++)
            {
                id += i;
                client1.ResetId(id);
                Assert.Equal(id, client1.Id);
            }
            id = string.Empty;
            for (var i = 0; i < 10; i++)
            {
                id += i;
                using (var byteBlock = new ByteBlock())
                {
                    byteBlock.Write(id);
                    client1.Send(10001, byteBlock);
                }
                Thread.Sleep(50);
                Assert.Equal(id, client1.Id);
            }

            var id1 = client1.Id;

            Assert.ThrowsAny<Exception>(() =>
            {
                client1.ResetId(client2.Id);
            });

            Assert.Equal(client1.Id, id1);

            client2.Ping(client1.Id);
            client1.Ping(client2.Id);
        }

        [Theory]
        [InlineData(SerializationType.FastBinary)]
        [InlineData(SerializationType.Json)]
        [InlineData(SerializationType.Xml)]
        public async void ReverseInvokeServiceShouldBeOk(SerializationType serializationType)
        {
            var client = await this.GetClient(this.GetConfig());

           
            var invokeOption = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = serializationType,
                Timeout = 1000
            };

            var remoteTest = new RemoteTest(client.GetDmtpRpcActor());

            remoteTest.Test19(client.Id, invokeOption);
        }

        private async Task<WebSocketDmtpClient> GetClient(TouchSocketConfig config, bool con = true)
        {
            var client = new WebSocketDmtpClient();

            client.Setup(config);
            if (con)
            {
                await client.ConnectAsync();
            }
            return client;
        }

        private TouchSocketConfig GetConfig()
        {
            return new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:7806/websocketsmtp")
                .SetVerifyToken("123RPC")
                .SetCacheTimeoutEnable(false)
                .ConfigureContainer(a =>
                {
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<CallbackServer>();
                    });
                    a.UseDmtpFileTransfer();
                    a.UseDmtpRedis();
                    a.UseDmtpRemoteAccess();
                    a.UseDmtpRemoteStream();

                    a.Add<MyDmtpPlugin>();
                    //// a.use<TcpDmtpClient>()
                    // .SetInterval(TimeSpan.FromSeconds(1))
                    // .SetMaxFailCount(5);
                });
        }
    }
}