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

using System.IO.Compression;

namespace TouchSocket.Http;

/// <summary>
/// Http静态内容插件
/// </summary>
[PluginOption(Singleton = false)]
public sealed class HttpStaticPagePlugin : PluginBase, IHttpPlugin
{
    private readonly StaticFilesPool m_filesPool;

    /// <summary>
    /// 构造函数
    /// </summary>
    public HttpStaticPagePlugin(StaticPageOptions options)
    {
        this.m_filesPool = options.FilesPool;
        this.NavigateAction = options.NavigateAction;
        this.ResponseAction = options.ResponseAction;
        this.ContentTypeProvider = options.ContentTypeProvider;
    }

    /// <summary>
    /// 提供文件扩展名和MIME类型之间的映射。
    /// </summary>
    public IContentTypeProvider ContentTypeProvider { get; }

    /// <summary>
    /// 重新导航
    /// </summary>
    public Func<HttpRequest, Task<string>> NavigateAction { get; }

    /// <summary>
    /// 在响应之前调用。
    /// </summary>
    public Func<HttpContext, Task> ResponseAction { get; }

    /// <summary>
    /// 静态文件池
    /// </summary>
    public StaticFilesPool StaticFilesPool => this.m_filesPool;

    /// <summary>
    /// 添加静态文件目录
    /// </summary>
    /// <param name="path">Static content path</param>
    /// <param name="prefix">Cache prefix (default is "/")</param>
    /// <param name="filter">Cache filter (default is "*.*")</param>
    /// <param name="timeout">Refresh cache millisecondsTimeout (default is 1 hour)</param>
    public void AddFolder(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = default)
    {
        this.m_filesPool.AddFolder(path, prefix, filter, timeout);
    }

    /// <summary>
    /// 移除所有静态页面
    /// </summary>
    public void ClearFolder()
    {
        this.m_filesPool.Clear();
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var url = await this.NavigateAction.Invoke(e.Context.Request).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (this.m_filesPool.TryFindEntry(url, out var staticEntry))
        {
            var request = e.Context.Request;
            var response = e.Context.Response;
            response.SetStatusWithSuccess();
            if (this.ContentTypeProvider?.TryGetContentType(url, out var result) != true)
            {
                result = HttpTools.GetContentTypeFromExtension(url);
            }
            response.ContentType = result;

            if (staticEntry.IsCacheBytes)
            {
                var data = staticEntry.Value;

                if (request.IsAcceptGzip())
                {
                    using (var byteBlock = new ByteBlock(data.Length))
                    {
                        using (var zipStream = new GZipStream(byteBlock.AsStream(false), CompressionMode.Compress, true))
                        {
                            zipStream.Write(data, 0, data.Length);
                            zipStream.Close();

                            var resultData = byteBlock.ToArray();
                            response.SetGzipContent(resultData);
                        }
                    }
                }
                else
                {
                    response.SetContent(data);
                }
            }
            if (this.ResponseAction != null)
            {
                await this.ResponseAction.Invoke(e.Context).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            if (staticEntry.IsCacheBytes)
            {
                await response.AnswerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                await response.FromFileAsync(staticEntry.FileInfo, request).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            e.Handled = true;
        }
        else
        {
            await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 移除指定路径的静态文件
    /// </summary>
    /// <param name="path">Static content path</param>
    public void RemoveFolder(string path)
    {
        path = FileUtility.PathFormat(path);
        path = path.EndsWith("/") ? path : path + "/";

        this.m_filesPool.RemoveFolder(path);
    }
}