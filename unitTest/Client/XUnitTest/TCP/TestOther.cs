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
using System.Linq;
using TouchSocket.Core.Config;
using TouchSocket.Core.Log;
using TouchSocket.Sockets;
using Xunit;

namespace XUnitTest.TCP
{
    public class TestOther
    {
        [Fact]
        public void OfRRQMConfigBeOk()
        {
            int bufferLength = 1024;
            ConsoleLogger logger = new ConsoleLogger();
            TouchSocketConfig config = new TouchSocketConfig();
            config.SetValue(TouchSocketConfigExtension.BufferLengthProperty, bufferLength);

            Assert.Equal(bufferLength, config.GetValue<int>(TouchSocketConfigExtension.BufferLengthProperty));
        }

        [Fact]
        public void IPHostShouldBeOk()
        {
            IPHost iPHost_1 = new IPHost("127.0.0.1:7789");
            Assert.NotNull(iPHost_1);
            Assert.Equal("127.0.0.1", iPHost_1.IP);
            Assert.Equal(7789, iPHost_1.Port);

            IPHost iPHost_2 = new IPHost(7789);
            Assert.NotNull(iPHost_2);
            Assert.Equal("0.0.0.0", iPHost_2.IP);
            Assert.Equal(7789, iPHost_2.Port);

            IPHost iPHost_3 = new IPHost(System.Net.IPAddress.Parse("127.0.0.1"), 7789);
            Assert.NotNull(iPHost_3);
            Assert.Equal("127.0.0.1", iPHost_3.IP);
            Assert.Equal(7789, iPHost_3.Port);
        }

        [Fact]
        public void TLVDataFrameShouldOk()
        {
            TLVDataFrame requestInfo = new TLVDataFrame();
            Assert.True(requestInfo.Tag == 0);
            Assert.True(requestInfo.Length == 0);
            Assert.Null(requestInfo.Value);

            requestInfo = new TLVDataFrame(10, new byte[] { 0, 1, 2, 3 });
            Assert.True(requestInfo.Tag == 10);
            Assert.True(requestInfo.Length == 4);
            Assert.True(requestInfo.Value.SequenceEqual(new byte[] { 0, 1, 2, 3 }));

            Assert.ThrowsAny<Exception>(() =>
            {
                requestInfo.AppendValue(new byte[] { 0, 1, 2, 3 });
            });

            requestInfo.ClearValue();
            requestInfo.Tag = 10;
            requestInfo.AppendValue(new byte[] { 0, 1, 2, 3 });
            Assert.True(requestInfo.Tag == 10);
            Assert.True(requestInfo.Length == 4);
            Assert.True(requestInfo.Value.SequenceEqual(new byte[] { 0, 1, 2, 3 }));

            requestInfo.AppendValue(new byte[] { 4, 5, 6, 7 });
            Assert.True(requestInfo.Length == 8);
            Assert.True(requestInfo.Value.SequenceEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }));
        }

        [Fact]
        public void ValueTLVDataFrameShouldOk()
        {
            ValueTLVDataFrame requestInfo = new ValueTLVDataFrame();
            Assert.True(requestInfo.Tag == 0);
            Assert.True(requestInfo.Length == 0);
            Assert.Null(requestInfo.Value);

            requestInfo = new ValueTLVDataFrame(10, new byte[] { 0, 1, 2, 3 });
            Assert.True(requestInfo.Tag == 10);
            Assert.True(requestInfo.Length == 4);
            Assert.True(requestInfo.Value.SequenceEqual(new byte[] { 0, 1, 2, 3 }));

            Assert.ThrowsAny<Exception>(() =>
            {
                requestInfo.AppendValue(new byte[] { 0, 1, 2, 3 });
            });

            requestInfo.ClearValue();
            requestInfo.Tag = 10;
            requestInfo.AppendValue(new byte[] { 0, 1, 2, 3 });
            Assert.True(requestInfo.Tag == 10);
            Assert.True(requestInfo.Length == 4);
            Assert.True(requestInfo.Value.SequenceEqual(new byte[] { 0, 1, 2, 3 }));

            requestInfo.AppendValue(new byte[] { 4, 5, 6, 7 });
            Assert.True(requestInfo.Length == 8);
            Assert.True(requestInfo.Value.SequenceEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }));
        }
    }
}