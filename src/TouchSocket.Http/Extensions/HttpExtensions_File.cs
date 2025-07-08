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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;

public static partial class HttpExtensions
{
    /// <summary>
    /// 从文件响应。
    /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
    /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
    /// </summary>
    /// <param name="response">响应</param>
    /// <param name="request">请求头，用于尝试续传，为<see langword="null"/>时则不续传。</param>
    /// <param name="fileInfo">文件信息</param>
    /// <param name="flowOperator">流速控制</param>
    /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
    /// <param name="autoGzip">是否自动<see cref="HttpRequest"/>请求，自动启用gzip</param>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    public static async Task<Result> FromFileAsync(this HttpResponse response, FileInfo fileInfo, HttpFlowOperator flowOperator, HttpRequest request = default, string fileName = null, bool autoGzip = true)
    {
        try
        {
            var bufferLen = flowOperator.BlockSize;

            var filePath = fileInfo.FullName;
            using (var streamReader = File.OpenRead(filePath))
            {
                response.SetContentTypeByExtension(Path.GetExtension(filePath));
                if (fileName.HasValue())
                {
                    var contentDisposition = "attachment;" + "filename=" + System.Web.HttpUtility.UrlEncode(fileName ?? Path.GetFileName(filePath));
                    response.Headers.TryAdd(HttpHeaders.ContentDisposition, contentDisposition);
                }

                response.Headers.TryAdd(HttpHeaders.AcceptRanges, "bytes");

                autoGzip = autoGzip && request.IsAcceptGzip();

                HttpRange httpRange;
                var range = request?.Headers.Get(HttpHeaders.Range);
                if (string.IsNullOrEmpty(range))
                {
                    response.SetStatusWithSuccess();
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

                if (autoGzip)
                {
                    response.IsChunk = true;
                    response.Headers.Add(HttpHeaders.ContentEncoding, "gzip");
                }
                else
                {
                    flowOperator.AddCompletedLength(httpRange.Start);
                    flowOperator.SetLength(fileInfo.Length);
                }

                var buffer = ArrayPool<byte>.Shared.Rent(bufferLen);
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
                                    var r = await streamReader.ReadAsync(buffer, 0, bufferLen).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                    if (r == 0)
                                    {
                                        gzip.Close();
                                        await response.CompleteChunkAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                        break;
                                    }

                                    await flowOperator.AddFlowAsync(r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                                    await gzip.WriteAsync(buffer, 0, r, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                }
                            }
                        }
                    }
                    else
                    {
                        var surLen = httpRange.Length;
                        while (surLen > 0)
                        {
                            var r = await streamReader.ReadAsync(buffer, 0, (int)Math.Min(bufferLen, surLen)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            if (r == 0)
                            {
                                break;
                            }

                            await flowOperator.AddFlowAsync(r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                            await response.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, r)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            surLen -= r;
                        }

                        if (response.IsChunk)
                        {
                            await response.CompleteChunkAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            return flowOperator.SetResult(Result.Success);
        }
        catch (Exception ex)
        {
            return flowOperator.SetResult(Result.FromException(ex));
        }
    }


    ///<summary>
    /// 从文件响应。
    /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
    /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
    /// </summary>
    /// <param name="response">响应</param>
    /// <param name="fileInfo">文件信息</param>
    /// <param name="request">请求头，用于尝试续传，为<see langword="null"/>时则不续传。</param>
    /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
    /// <param name="maxSpeed">最大速度。</param>
    /// <param name="bufferLen">读取长度。</param>
    /// <param name="autoGzip">是否自动<see cref="HttpRequest"/>请求，自动启用GZip</param>
    /// <exception cref="Exception"></exception>
    /// <returns>异步任务</returns>
    public static async Task FromFileAsync(this HttpResponse response, FileInfo fileInfo, HttpRequest request = default, string fileName = null, int maxSpeed = int.MaxValue, int bufferLen = 1024 * 64, bool autoGzip = true)
    {
        var flowOperator = new HttpFlowOperator { BlockSize = bufferLen, MaxSpeed = maxSpeed };
        var result = await FromFileAsync(response, fileInfo, flowOperator, request, fileName, autoGzip).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (!result.IsSuccess)
        {
            throw new Exception(result.Message);
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
    public static async Task FromFileAsync(this HttpContext context, FileInfo fileInfo, string fileName = null, int maxSpeed = int.MaxValue, int bufferLen = 1024 * 64, bool autoGzip = true)
    {
        await FromFileAsync(context.Response, fileInfo, context.Request, fileName, maxSpeed, bufferLen, autoGzip).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

}