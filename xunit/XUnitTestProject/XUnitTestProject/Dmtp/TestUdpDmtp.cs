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
using TouchSocket.Dmtp.RouterPackage;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using XUnitTestProject.Dmtp._Packages;
using XUnitTestProject.Rpc;

namespace XUnitTestProject.Dmtp
{
    public class TestUdpDmtp : UnitBase
    {
        private UdpDmtp client;

        [Fact]
        public void CancellationTokenInvokeShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());
            var remoteTest = new RemoteTest(client.GetDmtpRpcActor());
            remoteTest.Test26();
        }

        [Fact]
        public void CompressRemoteStreamShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());
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
        public void CreateChannelToServerShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());

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
                var buffer = new byte[1024];
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
        public void InvokeServiceShouldBeOk(SerializationType serializationType)
        {
            var client = this.GetClient(this.GetConfig());

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
        public async Task PingAsyncShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());

            for (var i = 0; i < 1000; i++)
            {
                Assert.True(await client.PingAsync());
            }
        }

        [Fact]
        public void PingShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());

            for (var i = 0; i < 100000; i++)
            {
                Assert.True(client.Ping());
            }
        }

        [Fact]
        public void PullFileShouldBeOk()
        {
            for (var j = 0; j < 10; j++)
            {
                var client = this.GetClient(this.GetConfig());
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

                var result = client.GetDmtpFileTransferActor().PullFile(new FileOperator()
                {
                    ResourcePath = Path.GetFullPath(path),
                    SavePath = savePath,
                    FileSectionSize = 1024
                });

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
        public void PullSmallFileShouldBeOk()
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
                var buffer = new byte[new Random().Next(1024, 1024 * 10)];
                new Random().NextBytes(buffer);
                writer.Write(buffer);
            }

            var client = this.GetClient(this.GetConfig());
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
        public void PushFileShouldBeOk()
        {
            for (var j = 0; j < 10; j++)
            {
                var client = this.GetClient(this.GetConfig());
                var path = this.GetType().Name + "PushFile.test";
                var savePath = this.GetType().Name + "SavePushFile.test";
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
                    for (var i = 0; i < 1; i++)
                    {
                        var buffer = new byte[1024 * 1024];
                        new Random().NextBytes(buffer);
                        //byte[] buffer = new byte[] {0,1,2,3,4,5,6,7,8,9 };
                        writer.Write(buffer);
                    }
                }

                var result = client.GetDmtpFileTransferActor().PushFile(new FileOperator()
                {
                    ResourcePath = path,
                    SavePath = Path.GetFullPath(savePath),
                    FileSectionSize = 1024
                });

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
        public void PushSmallFileShouldBeOk()
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
                var buffer = new byte[new Random().Next(1024, 1024 * 10)];
                new Random().NextBytes(buffer);
                writer.Write(buffer);
            }

            var client = this.GetClient(this.GetConfig());
            var result = client.GetDmtpFileTransferActor().PushSmallFile(Path.GetFullPath(savePath), new FileInfo(path), default, timeout: 10000);
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
        public void RedisShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());
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
                var buffer = new byte[1024];
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
        public void RemoteAccessShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());
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
        public void RemoteStreamShouldBeOk()
        {
            var client = this.GetClient(this.GetConfig());

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
        public void RouterPackageShouldBeOk()
        {
            var client1 = this.GetClient(this.GetConfig());

            //DmtpRouterPackageActor
            var actor1 = client1.GetDmtpRouterPackageActor();

            var testPackage = new TestPackage();
            for (var i = 0; i < 1000; i++)
            {
                var package = actor1.Request<TestPackage>(testPackage);
                Assert.NotNull(package);
                Assert.True(package.Status == 1);
                Assert.True(package.Message == "message");
            }
        }

        private UdpDmtp GetClient(TouchSocketConfig config)
        {
            if (this.client != null)
            {
                return this.client;
            }
            this.client = new UdpDmtp();

            this.client.Setup(config);
            this.client.Start();
            return this.client;
        }

        private TouchSocketConfig GetConfig()
        {
            return new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7797")
                .UseUdpReceive()
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
                    a.UseDmtpRouterPackage();

                    a.Add<MyDmtpPlugin>();
                });
        }
    }
}