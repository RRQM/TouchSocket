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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    public static class HttpClientExtension
    {
        public static async Task<byte[]> GetByteArrayAsync(this IHttpClient httpClient, string url, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            var request = new HttpRequest();
            request.Method = HttpMethod.Get;
            request.SetUrl(url);

            using (var responseResult = await httpClient.RequestAsync(request, millisecondsTimeout, token).ConfigureFalseAwait())
            {
                var response = responseResult.Response;
                if (!response.IsSuccess())
                {
                    ThrowHelper.ThrowException(response.StatusMessage);
                }

                return await response.GetContentAsync(token);
            }
        }

        public static async Task<string> GetStringAsync(this IHttpClient httpClient, string url, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            return (await GetByteArrayAsync(httpClient, url, millisecondsTimeout, token)).ToUtf8String();
        }

        #region Download

        public static async Task GetFileAsync(this IHttpClient httpClient, HttpRequest request, Stream stream, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            using (var responseResult = await httpClient.RequestAsync(request, millisecondsTimeout, token))
            {
                var response = responseResult.Response;

                await response.ReadCopyToAsync(stream, token);
            }
        }

        public static Task GetFileAsync(this IHttpClient httpClient, string url, Stream stream, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            var request = new HttpRequest();
            request.InitHeaders();
            request.SetUrl(url);
            return GetFileAsync(httpClient, request, stream, millisecondsTimeout, token);
        }

        #endregion Download
    }
}