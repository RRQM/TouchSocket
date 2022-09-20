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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Http;
using Xunit;

namespace XUnitTest.Http
{
    public class TestHttp: UnitBase
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
            HttpRange[] httpRanges = HttpRange.GetRanges(str, 1000);
            Assert.Equal(count, httpRanges.Length);
        }

        [Fact]
        public void HttpQueuyShouldBeOk()
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetUrl("/a?a=10")
                .InitHeaders()
                .AsGet();
            for (int i = 0; i < 10; i++)
            {
                httpRequest.Query.Add(i.ToString(), i.ToString());
            }

            using (ByteBlock byteBlock = new ByteBlock())
            {
                httpRequest.Build(byteBlock);
                string s = byteBlock.ToString();

                HttpRequest request = new HttpRequest();
                request.ParsingHeader(byteBlock.SeekToStart(), byteBlock.CanReadLen);

                Assert.Equal(11, request.Query.Count);

                Assert.True(request.Query.Get("a")=="10");
            }
        }

        [Fact]
        public void HttpQueuy2ShouldBeOk()
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetUrl("/abc")
                .InitHeaders()
                .AsGet();

            Assert.Equal("/abc", httpRequest.URL);
            Assert.Equal("/abc", httpRequest.RelativeURL);
        }

        [Fact]
        public async void HttpRequestShouldBeOk()
        {
            ThreadPool.SetMinThreads(50, 50);
            HttpClient client = new HttpClient();
            client.Setup("127.0.0.1:7801");
            client.Connect();

            await Task.Run(() =>
             {
                 for (int i = 0; i < 100000; i++)
                 {
                     HttpRequest httpRequest = new HttpRequest();
                     httpRequest
                         .InitHeaders()
                         .SetUrl("/xunit")
                         .AsGet();

                     var response = client.RequestContent(httpRequest);
                     Assert.Equal("OK", response.GetBody());
                 }
             });
        }
    }
}