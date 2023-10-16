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
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace XUnitTestProject.Http
{
    public class TestHttp : UnitBase
    {
        [Theory]
        [InlineData("bytes=0-499", 1)]
        [InlineData("bytes=500-999", 1)]
        [InlineData("bytes=-500", 1)]
        [InlineData("bytes=500-", 1)]
        [InlineData("bytes=0-0,-1", 2)]
        [InlineData("bytes=500-600,601-999", 2)]
        public void HttpRangeShouldBeOk(string str, int count)
        {
            var httpRanges = HttpRange.GetRanges(str, 1000);
            Assert.Equal(count, httpRanges.Length);
        }

        [Fact]
        public void HttpQueuyShouldBeOk()
        {
            var httpRequest = new HttpRequest();
            httpRequest.SetUrl("/a?a=10")
                .InitHeaders()
                .AsGet();
            for (var i = 0; i < 10; i++)
            {
                httpRequest.Query.Add(i.ToString(), i.ToString());
            }

            using (var byteBlock = new ByteBlock())
            {
                httpRequest.Build(byteBlock);
                var s = byteBlock.ToString();

                var request = new HttpRequest();
                byteBlock.SeekToStart();
                request.ParsingHeader(byteBlock, byteBlock.CanReadLen);

                Assert.Equal(11, request.Query.Count);

                Assert.True(request.Query.Get("a") == "10");
            }
        }

        [Fact]
        public void HttpQueuy2ShouldBeOk()
        {
            var httpRequest = new HttpRequest();
            httpRequest.SetUrl("/abc")
                .InitHeaders()
                .AsGet();

            Assert.Equal("/abc", httpRequest.URL);
            Assert.Equal("/abc", httpRequest.RelativeURL);
        }

        [Fact]
        public void HttpRequestShouldBeOk()
        {
            var client = new TouchSocket.Http.HttpClient();
            client.Connect("127.0.0.1:7801");

            for (var i = 0; i < 100000; i++)
            {
                var httpRequest = new HttpRequest();
                httpRequest
                    .InitHeaders()
                    .SetUrl("/xunit")
                    .AsGet();

                var response = client.RequestContent(httpRequest);
                Assert.Equal("OK", response.GetBody());
            }
        }

        [Fact]
        public void HttpHeaderShouldBeOk()
        {
            var httpRequest = new HttpRequest();
            httpRequest.InitHeaders();

            Assert.True(httpRequest.Headers.Count == 5);

            httpRequest.Headers["header"] = "header";
            Assert.True(httpRequest.Headers.Count == 6);

            Assert.True(httpRequest.Headers["header"] == "header");
            Assert.True(httpRequest.Headers["Header"] == "header");
            Assert.True(httpRequest.Headers["HeAder"] == "header");
            Assert.True(httpRequest.Headers["header"] != "Header");

            httpRequest.Headers.Add(HttpHeaders.Age, "Age");
        }
    }
}