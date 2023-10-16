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
using RpcProxy;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace XUnitTestProject.Rpc
{
    public class RemoteTest
    {
        private readonly XUnitTestController server;

        public RemoteTest(IRpcClient client)
        {
            this.server = new XUnitTestController(client);
        }

        public void Test01(IInvokeOption invokeOption)
        {
            this.server.Test01_Performance(invokeOption);
        }

        public void Test02(IInvokeOption invokeOption)
        {
            for (var i = 0; i < 10; i++)
            {
                var returnValue = this.server.Test02_TaskString(i.ToString(), invokeOption);
                Assert.Equal(i.ToString(), returnValue);
            }
        }

        public void Test03(IInvokeOption invokeOption)
        {
            var proxyClass1 = this.server.Test03_GetProxyClass(invokeOption);
            Assert.Equal(10, proxyClass1.P1);
            Assert.Equal(100, proxyClass1.P2.P1);
            Assert.Equal(1000, proxyClass1.P2.P2.P1);
        }

        public void Test04(IInvokeOption invokeOption)
        {
            var value = this.server.Test04_In32DefaultValue(invokeOption: invokeOption);
            Assert.Equal(100, value);

            value = this.server.Test04_In32DefaultValue(int.MaxValue, invokeOption: invokeOption);
            Assert.Equal(int.MaxValue, value);
        }

        public void Test05(IInvokeOption invokeOption)
        {
            this.server.Test05_NoneReturnNoneParameter(invokeOption: invokeOption);
        }

        public void Test06(IInvokeOption invokeOption)
        {
            this.server.Test06_OutParameters(out var name, out var age, out var occupation, invokeOption: invokeOption);
            Assert.Equal("若汝棋茗", name);
            Assert.Equal(18, age);
            Assert.Equal("搬砖工程师", occupation);
        }

        public void Test07(IInvokeOption invokeOption)
        {
            this.server.Test07_OutStringParameter(out var name, invokeOption: invokeOption);
            Assert.Equal("若汝棋茗", name);
        }

        public void Test08(IInvokeOption invokeOption)
        {
            var name = "若汝棋茗";
            this.server.Test08_RefStringParameter(ref name, invokeOption: invokeOption);
            Assert.Equal("若汝棋茗ref", name);
        }

        public void Test09(IInvokeOption invokeOption)
        {
            Assert.True(this.server.Test09_Boolean(true, invokeOption: invokeOption));
            Assert.False(this.server.Test09_Boolean(false, invokeOption: invokeOption));
        }

        public void Test10(IInvokeOption invokeOption)
        {
            Assert.Null(this.server.Test10_StringDefaultNullValue(invokeOption: invokeOption));
        }

        public void Test11(IInvokeOption invokeOption)
        {
            Assert.Equal("RRQM", this.server.Test11_StringDefaultValue(invokeOption: invokeOption));
        }

        public void Test12(IInvokeOption invokeOption)
        {
            for (var i = 1; i < 10; i++)
            {
                var dic = this.server.Test12_Dictionary(i, invokeOption: invokeOption);
                Assert.Equal(i, dic.Count);
                for (var j = 0; j < dic.Count; j++)
                {
                    Assert.True(dic.ContainsKey(j));
                    Assert.Equal(j.ToString(), dic[j]);
                }
            }
        }

        public void Test13(IInvokeOption invokeOption)
        {
            this.server.Test13_Task(invokeOption: invokeOption);
        }

        public void Test14(IInvokeOption invokeOption)
        {
            for (var i = 1; i < 10; i++)
            {
                var list = this.server.Test14_ListClass01(i, invokeOption: invokeOption);
                Assert.True(list.Count == i);
                for (var j = 0; j < list.Count; j++)
                {
                    Assert.Equal(j, list[j].Age);
                }
            }
        }

        public void Test15(IInvokeOption invokeOption)
        {
            var args = this.server.Test15_ReturnArgs(invokeOption: invokeOption);
            Assert.NotNull(args);
            Assert.Equal(10, args.P1);
            Assert.Equal(10.0, args.P2);
            Assert.Equal("RRQM", args.P3);
        }

        public void Test16(IInvokeOption invokeOption)
        {
            var class04 = this.server.Test16_ReturnClass4(10, "RRQM", invokeOption: invokeOption);
            Assert.NotNull(class04);
            Assert.Equal(10, class04.P1);
            Assert.Equal("RRQM", class04.P2);
            Assert.Equal(10, class04.P3);
        }

        public void Test17(IInvokeOption invokeOption)
        {
            Assert.Equal(3.1415926, this.server.Test17_DoubleDefaultValue(invokeOption: invokeOption));
            Assert.Equal(10.0, this.server.Test17_DoubleDefaultValue(10.0, invokeOption: invokeOption));
        }

        public void Test18(IInvokeOption invokeOption)
        {
            var class01 = this.server.Test18_Class1(new Class01() { Age = 10, Name = "RRQM" }, invokeOption: invokeOption);
            Assert.NotNull(class01);
            Assert.Equal(10, class01.Age);
            Assert.Equal("RRQM", class01.Name);
        }

        public void Test19(string id, IInvokeOption invokeOption)
        {
            for (var i = 0; i < 10; i++)
            {
                var msg = this.server.Test19_CallBacktcpRpcService(id, i, invokeOption: invokeOption);
                Assert.Equal($"我今年{i}岁了", msg);
            }
        }

        public void Test22(IInvokeOption invokeOption)
        {
            var value = this.server.Test22_IncludeCaller(10, invokeOption: invokeOption);
            Assert.Equal(10, value);
        }

        public void Test25(IInvokeOption invokeOption)
        {
            var result = this.server.Test25_TestStruct(new StructArgs() { P1 = 10 }, invokeOption: invokeOption);
            Assert.Equal(10, result);
        }

        public void Test26()
        {
            var invokeOption1 = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = SerializationType.FastBinary,
                Timeout = 20 * 1000
            };

            var result = this.server.Test26_TestCancellationToken(invokeOption1);
            Assert.Equal(1, result);

            var tokenSource = new CancellationTokenSource();

            var invokeOption2 = new DmtpInvokeOption()
            {
                Token = tokenSource.Token,
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = SerializationType.FastBinary,
                Timeout = 20 * 1000
            };

            var t = new Random().Next(2, 15);

            var timeSpan = TimeMeasurer.Run(() =>
              {
                  _ = Task.Run(async () =>
                  {
                      await Task.Delay(t * 1000);
                      tokenSource.Cancel();
                  });
                  var result2 = this.server.Test26_TestCancellationToken(invokeOption2);
                  Assert.Equal(0, result2);
              });
            Assert.True(timeSpan > TimeSpan.FromSeconds(t));
        }

        public void Test30(string id, IInvokeOption invokeOption)
        {
            for (var i = 0; i < 10; i++)
            {
                var msg = this.server.Test30_CallBackHttpRpcService(id, i, invokeOption: invokeOption);
                Assert.Equal($"我今年{i}岁了", msg);
            }
        }

        public void Test38(string id, IInvokeOption invokeOption)
        {
            for (var i = 0; i < 10; i++)
            {
                var msg = this.server.Test38_CallBackJsonRpcService(id, i, invokeOption: invokeOption);
                Assert.Equal($"我今年{i}岁了", msg);
            }
        }
    }
}