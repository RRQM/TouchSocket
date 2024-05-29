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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http静态内容插件
    /// </summary>
    [PluginOption(Singleton = false)]
    public class HttpStaticPagePlugin : PluginBase
    {
        private readonly StaticFilesPool m_fileCache;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpStaticPagePlugin()
        {
            this.m_fileCache = new StaticFilesPool();
            this.SetNavigateAction(request =>
            {
                var relativeURL = request.RelativeURL;
                var url = relativeURL;

                if (this.m_fileCache.ContainsEntry(url))
                {
                    return url;
                }

                if (relativeURL.EndsWith("/"))
                {
                    url = relativeURL + "index.html";
                    if (this.m_fileCache.ContainsEntry(url))
                    {
                        return url;
                    }
                }
                else if (relativeURL.EndsWith("index"))
                {
                    url = relativeURL + ".html";
                    if (this.m_fileCache.ContainsEntry(url))
                    {
                        return url;
                    }
                }
                else
                {
                    url = relativeURL + "/index.html";
                    if (this.m_fileCache.ContainsEntry(url))
                    {
                        return url;
                    }
                }
                return relativeURL;
            });
        }

        /// <summary>
        /// 提供文件扩展名和MIME类型之间的映射。
        /// </summary>
        public IContentTypeProvider ContentTypeProvider { get; set; }

        /// <summary>
        /// 静态文件池
        /// </summary>
        public StaticFilesPool StaticFilesPool => this.m_fileCache;

        /// <summary>
        /// 重新导航
        /// </summary>
        public Func<HttpRequest, Task<string>> NavigateAction { get; set; }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        public Func<HttpContext, Task> ResponseAction { get; set; }

        /// <summary>
        /// 添加静态文件目录
        /// </summary>
        /// <param name="path">Static content path</param>
        /// <param name="prefix">Cache prefix (default is "/")</param>
        /// <param name="filter">Cache filter (default is "*.*")</param>
        /// <param name="timeout">Refresh cache millisecondsTimeout (default is 1 hour)</param>
        public void AddFolder(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = default)
        {
            this.m_fileCache.AddFolder(path, prefix, filter, timeout);
        }

        /// <summary>
        /// 设置提供文件扩展名和MIME类型之间的映射。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetContentTypeProvider(IContentTypeProvider provider)
        {
            this.ContentTypeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            return this;
        }

        /// <summary>
        /// 设定重新导航
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetNavigateAction(Func<HttpRequest, Task<string>> func)
        {
            this.NavigateAction = func;
            return this;
        }

        /// <summary>
        /// 设定重新导航
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetNavigateAction(Func<HttpRequest, string> func)
        {
            this.NavigateAction = (request) =>
            {
                return Task.FromResult(func(request));
            };
            return this;
        }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetResponseAction(Func<HttpContext, Task> func)
        {
            this.ResponseAction = func;
            return this;
        }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetResponseAction(Action<HttpContext> action)
        {
            this.ResponseAction = (response) =>
            {
                action.Invoke(response);
                return EasyTask.CompletedTask;
            };
            return this;
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            pluginManager.Add<IHttpSessionClient, HttpContextEventArgs>(typeof(IHttpPlugin), this.OnHttpRequest);
            base.Loaded(pluginManager);
        }

        #region Remove

        /// <summary>
        /// 移除所有静态页面
        /// </summary>
        public void ClearFolder()
        {
            this.m_fileCache.Clear();
        }

        /// <summary>
        /// 移除指定路径的静态文件
        /// </summary>
        /// <param name="path">Static content path</param>
        public void RemoveFolder(string path)
        {
            path = FileUtility.PathFormat(path);
            path = path.EndsWith("/") ? path : path + "/";

            this.m_fileCache.RemoveFolder(path);
        }

        #endregion Remove

        private async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            var url = await this.NavigateAction.Invoke(e.Context.Request).ConfigureFalseAwait();
            if (this.m_fileCache.TryFindEntry(url, out var staticEntry))
            {
                var request = e.Context.Request;
                var response = e.Context.Response;
                response.SetStatus();
                if (this.ContentTypeProvider?.TryGetContentType(url, out var result) != true)
                {
                    result = HttpTools.GetContentTypeFromExtension(url);
                }
                response.ContentType = result;

                if (staticEntry.IsCacheBytes)
                {
                    response.SetContent(staticEntry.Value);
                }
                if (this.ResponseAction != null)
                {
                    await this.ResponseAction.Invoke(e.Context).ConfigureFalseAwait();
                }

                if (staticEntry.IsCacheBytes)
                {
                    await response.AnswerAsync().ConfigureFalseAwait();
                }
                else
                {
                    await response.FromFileAsync(staticEntry.FileInfo.FullName, request).ConfigureFalseAwait();
                }
                e.Handled = true;
            }
            else
            {
                await e.InvokeNext().ConfigureFalseAwait();
            }
        }
    }
}