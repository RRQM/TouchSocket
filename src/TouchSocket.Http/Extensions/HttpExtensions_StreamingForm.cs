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

namespace TouchSocket.Http;

public static partial class HttpExtensions
{
    /// <summary>
    /// 获取流式表单读取器，支持逐段拉取 <c>multipart/form-data</c> 或 <c>application/x-www-form-urlencoded</c> 表单数据。
    /// <para>无论文件大小均不会将请求体整体加载到内存。使用完毕后必须调用 <see cref="IDisposable.Dispose"/> 或配合 <see langword="using"/> 语句释放资源。</para>
    /// </summary>
    /// <typeparam name="TRequest">请求类型，必须继承自 <see cref="HttpRequest"/>。</typeparam>
    /// <param name="request">HTTP 请求对象。</param>
    /// <returns>
    /// 实现 <see cref="IStreamingFormReader"/> 的读取器实例。调用 <see cref="IStreamingFormReader.ReadNextSectionAsync"/>
    /// 依次获取每个表单段，返回 <see langword="null"/> 表示已无更多段。
    /// </returns>
    public static IStreamingFormReader GetStreamingFormReader<TRequest>(this TRequest request)
        where TRequest : HttpRequest
    {
        var boundaryString = request.GetBoundary();

        if (!boundaryString.IsNullOrEmpty())
        {
            return new InternalMultipartFormReader(request, boundaryString);
        }

        var contentType = request.ContentType;
        if (CheckFormBody(contentType, out var encoding))
        {
            return new InternalUrlEncodedFormReader(request, encoding);
        }

        return new InternalEmptyFormReader();
    }
}

