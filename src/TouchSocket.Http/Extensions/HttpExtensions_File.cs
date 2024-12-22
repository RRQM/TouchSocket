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
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    public static partial class HttpExtensions
    {
        /// <summary>
        /// 从文件响应。
        /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
        /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
        /// </summary>
        /// <param name="response">响应</param>
        /// <param name="request">请求头，用于尝试续传，为null时则不续传。</param>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
        /// <param name="maxSpeed">最大速度。</param>
        /// <param name="bufferLen">读取长度。</param>
        /// <param name="autoGzip">是否自动<see cref="HttpRequest"/>请求，自动启用gzip</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static async Task FromFileAsync(this HttpResponse response, FileInfo fileInfo, HttpRequest request = default, string fileName = null, int maxSpeed = 0, int bufferLen = 1024 * 64, bool autoGzip = true)
        {
            var filePath = fileInfo.FullName;
            using (var streamReader = File.OpenRead(filePath))
            {
                response.SetContentTypeByExtension(Path.GetExtension(filePath));
                if (fileName.HasValue())
                {
                    var contentDisposition = "attachment;" + "filename=" + System.Web.HttpUtility.UrlEncode(fileName ?? Path.GetFileName(filePath));
                    response.Headers.Add(HttpHeaders.ContentDisposition, contentDisposition);
                }

                response.Headers.Add(HttpHeaders.AcceptRanges, "bytes");

                autoGzip = autoGzip && request.IsAcceptGzip();

                HttpRange httpRange;
                var range = request?.Headers.Get(HttpHeaders.Range);
                if (string.IsNullOrEmpty(range))
                {
                    response.SetStatus();
                    if (!autoGzip)
                    {
                        response.ContentLength = fileInfo.Length;
                    }
                    httpRange = new HttpRange() { Start = 0, Length = fileInfo.Length };
                }
                else
                {
                    httpRange = HttpRange.GetRange(range, fileInfo.Length);
                    if (httpRange == null)
                    {
                        if (!autoGzip)
                        {
                            response.ContentLength = fileInfo.Length;
                        }
                        httpRange = new HttpRange() { Start = 0, Length = fileInfo.Length };
                    }
                    else
                    {
                        if (!autoGzip)
                        {
                            response.ContentLength = httpRange.Length;
                        }
                        response.SetStatus(206, "Partial Content");
                        response.Headers.Add(HttpHeaders.ContentRange, string.Format("bytes {0}-{1}/{2}", httpRange.Start, httpRange.Length + httpRange.Start - 1, fileInfo.Length));
                    }
                }

                streamReader.Position = httpRange.Start;
                var flowGate = new FlowGate
                {
                    Maximum = maxSpeed
                };

                if (autoGzip)
                {
                    response.IsChunk = true;
                    response.Headers.Add(HttpHeaders.ContentEncoding, "gzip");
                }

                var buffer = BytePool.Default.Rent(bufferLen);
                try
                {
                    if (autoGzip)
                    {
                        using (var responseStream = response.CreateWriteStream())
                        {
                            using (var gzip = new GZipStream(responseStream, CompressionMode.Compress, true))
                            {
                                while (true)
                                {
                                    var r = await streamReader.ReadAsync(buffer, 0, bufferLen).ConfigureAwait(false);
                                    if (r == 0)
                                    {
                                        gzip.Close();
                                        await response.CompleteChunkAsync().ConfigureAwait(false);
                                        break;
                                    }

                                    await flowGate.AddCheckWaitAsync(r).ConfigureAwait(false);

                                    await gzip.WriteAsync(buffer, 0, r, CancellationToken.None).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        var surLen = httpRange.Length;
                        while (surLen > 0)
                        {
                            var r = await streamReader.ReadAsync(buffer, 0, (int)Math.Min(bufferLen, surLen)).ConfigureAwait(false);
                            if (r == 0)
                            {
                                break;
                            }

                            await flowGate.AddCheckWaitAsync(r).ConfigureAwait(false);

                            await response.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, r)).ConfigureAwait(false);
                            surLen -= r;
                        }

                        if (response.IsChunk)
                        {
                            await response.CompleteChunkAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    BytePool.Default.Return(buffer);
                }
            }
        }

        /// <summary>
        /// 从文件响应。
        /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
        /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
        /// <param name="maxSpeed">最大速度。</param>
        /// <param name="bufferLen">读取长度。</param>
        /// <param name="autoGzip">是否自动<see cref="HttpRequest"/>请求，自动启用gzip</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static async Task FromFileAsync(this HttpContext context, FileInfo fileInfo, string fileName = null, int maxSpeed = 0, int bufferLen = 1024 * 64, bool autoGzip = true)
        {
            await FromFileAsync(context.Response, fileInfo, context.Request, fileName, maxSpeed, bufferLen, autoGzip);
        }

    }
}
