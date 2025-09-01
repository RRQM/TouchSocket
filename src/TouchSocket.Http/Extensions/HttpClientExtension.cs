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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// HttpClient 扩展类
/// </summary>
public static class HttpClientExtension
{
    /// <summary>
    /// 异步获取URL的字节数组表示形式。
    /// </summary>
    /// <param name="httpClient">发起HTTP请求的客户端。</param>
    /// <param name="url">要请求的URL。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <returns>包含从URL获取的字节的数组。</returns>
    /// <exception cref="Exception">如果HTTP请求失败，将抛出异常。</exception>
    public static async Task<byte[]> GetByteArrayAsync(this IHttpClient httpClient, string url, CancellationToken token)
    {
        // 创建HTTP请求对象
        var request = new HttpRequest();
        // 设置请求方法为GET
        request.Method = HttpMethod.Get;
        request.InitHeaders();
        request.SetHost(httpClient.RemoteIPHost.Host);
        // 设置请求URL
        request.URL = (url);

        // 使用指定的超时时间和取消令牌发起HTTP请求
        using (var responseResult = await httpClient.RequestAsync(request, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            // 获取HTTP响应
            var response = responseResult.Response;
            // 如果HTTP响应不是成功状态
            if (!response.IsSuccess())
            {
                // 抛出异常，包含响应状态信息
                ThrowHelper.ThrowException(response.StatusMessage);
            }

            // 读取响应内容并将其转换为字节数组返回
            return (await response.GetContentAsync(token)).ToArray();
        }
    }

    /// <summary>
    /// 异步获取指定URL的字符串内容。
    /// </summary>
    /// <param name="httpClient">用于发送HTTP请求的客户端。</param>
    /// <param name="url">要请求的URL。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <returns>返回从指定URL获取的字符串。</returns>
    public static async Task<string> GetStringAsync(this IHttpClient httpClient, string url, CancellationToken token)
    {
        // 将获取到的字节数组转换为UTF-8编码的字符串
        return (await GetByteArrayAsync(httpClient, url, token)).ToUtf8String();
    }

    #region Download

    /// <summary>
    /// 异步获取HTTP请求的文件内容
    /// </summary>
    /// <param name="httpClient">HTTP客户端接口</param>
    /// <param name="request">HTTP请求对象</param>
    /// <param name="stream">用于存储文件内容的目标流</param>
    /// <param name="token">用于取消操作的取消令牌</param>
    /// <returns>返回一个异步任务</returns>
    public static async Task GetFileAsync(this IHttpClient httpClient, HttpRequest request, Stream stream, CancellationToken token)
    {
        // 使用using语句确保响应对象正确地被释放
        using (var responseResult = await httpClient.RequestAsync(request, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            // 提取HTTP响应
            var response = responseResult.Response;

            // 将响应内容异步读取并复制到指定的流中
            await response.ReadCopyToAsync(stream, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 异步获取HTTP请求的文件内容
    /// </summary>
    /// <param name="httpClient">HTTP客户端接口</param>
    /// <param name="request">HTTP请求对象</param>
    /// <param name="stream">用于存储文件内容的目标流</param>
    /// <param name="flowOperator">用于控制下载过程的操作符。</param>
    /// <returns>返回一个Result对象，表示下载结果。</returns>
    public static async Task<Result> GetFileAsync(this IHttpClient httpClient, HttpRequest request, Stream stream, HttpFlowOperator flowOperator)
    {
        var token = flowOperator.Token;
       
        // 使用using语句确保响应对象正确地被释放
        using (var responseResult = await httpClient.RequestAsync(request, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            // 提取HTTP响应
            var response = responseResult.Response;

            // 将响应内容异步读取并复制到指定的流中
            return await response.ReadCopyToAsync(stream, flowOperator).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 异步获取URL指定的文件，并将其内容写入提供的流中。
    /// </summary>
    /// <param name="httpClient">用于发送HTTP请求的客户端。</param>
    /// <param name="url">要获取的文件的URL。</param>
    /// <param name="stream">将文件内容写入的流。</param>
    /// <param name="token">用于取消异步操作的取消令牌。</param>
    /// <returns>返回一个Task对象，表示异步操作。</returns>
    public static Task GetFileAsync(this IHttpClient httpClient, string url, Stream stream, CancellationToken token)
    {
        // 创建并初始化HttpRequest对象，用于封装HTTP请求的相关信息和操作
        var request = new HttpRequest();
        request.InitHeaders(); // 初始化请求头
        request.URL = (url); // 设置请求的URL
        request.SetHost(httpClient.RemoteIPHost.Host);
        // 调用重载的GetFileAsync方法，传入封装好的请求对象
        return GetFileAsync(httpClient, request, stream, token);
    }

    /// <summary>
    /// 异步获取URL指定的文件，并将其内容写入提供的流中。
    /// </summary>
    /// <param name="httpClient">用于发送HTTP请求的客户端。</param>
    /// <param name="url">要获取的文件的URL。</param>
    /// <param name="stream">将文件内容写入的流。</param>
    /// <param name="flowOperator">用于控制下载过程的操作符。</param>
    /// <returns>返回一个Result对象，表示下载结果。</returns>
    public static Task<Result> GetFileAsync(this IHttpClient httpClient, string url, Stream stream, HttpFlowOperator flowOperator)
    {
        // 创建并初始化HttpRequest对象，用于封装HTTP请求的相关信息和操作
        var request = new HttpRequest();
        request.InitHeaders(); // 初始化请求头
        request.URL = (url); // 设置请求的URL
        request.SetHost(httpClient.RemoteIPHost.Host);
        // 调用重载的GetFileAsync方法，传入封装好的请求对象
        return GetFileAsync(httpClient, request, stream, flowOperator);
    }

    #endregion Download

    #region Upload

    /// <summary>
    /// 异步上传文件到指定URL。
    /// </summary>
    /// <param name="client">HttpClient实例，用于发送HTTP请求。</param>
    /// <param name="url">文件上传的URL地址。</param>
    /// <param name="fileInfo">包含文件信息的FileInfo对象，用于获取文件内容和属性。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <typeparam name="TClient">客户端类型，必须继承自HttpClientBase并实现IHttpClient接口。</typeparam>
    public static async Task<Result> UploadFileAsync<TClient>(this TClient client, string url, FileInfo fileInfo, CancellationToken token)
        where TClient : HttpClientBase, IHttpClient
    {
        using (var stream = fileInfo.OpenRead())
        {
            //创建一个请求
            var request = new HttpRequest();
            request.InitHeaders();
            request.SetContent(new StreamHttpContent(stream));
            request.AddHeader("FileName", fileInfo.Name);
            request.URL = (url);
            request.SetHost(client.RemoteIPHost.Host);
            request.AsPost();

            using (var responseResult = await client.RequestAsync(request, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                var response = responseResult.Response;
                if (response.IsSuccess())
                {
                    return Result.Success;
                }
                return Result.FromFail(response.StatusMessage);
            }
        }
    }

    /// <summary>
    /// 异步上传文件到指定的URL。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须继承自HttpClientBase并实现IHttpClient接口。</typeparam>
    /// <param name="client">HttpClient实例，用于发送HTTP请求。</param>
    /// <param name="request">HTTP请求对象。</param>
    /// <param name="stream">包含文件内容的流。</param>
    /// <param name="flowOperator">用于控制上传过程的操作符。</param>
    /// <returns>返回一个Result对象，表示上传结果。</returns>
    public static async Task<Result> UploadFileAsync<TClient>(this TClient client, HttpRequest request, Stream stream, HttpFlowOperator flowOperator)
       where TClient : HttpClientBase, IHttpClient
    {
        try
        {
            var token = flowOperator.Token;
           
            //创建一个请求
            request.SetContent(new StreamHttpContent(stream, flowOperator));

            using (var responseResult = await client.RequestAsync(request, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                var response = responseResult.Response;
                if (response.IsSuccess())
                {
                    return flowOperator.SetResult(Result.Success);
                }
                return flowOperator.SetResult(Result.FromFail(response.StatusMessage));
            }
        }
        catch (Exception ex)
        {
            return flowOperator.SetResult(Result.FromException(ex));
        }
    }

    #endregion Upload
}