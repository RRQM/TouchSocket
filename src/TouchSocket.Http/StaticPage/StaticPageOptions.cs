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

using System.Threading.Tasks;
using System;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// 静态页面配置
    /// </summary>
    public class StaticPageOptions
    {
        public StaticPageOptions()
        {
            this.SetNavigateAction(request =>
            {
                var relativeURL = request.RelativeURL;
                var url = relativeURL;

                if (this.m_filesPool.ContainsEntry(url))
                {
                    return url;
                }

                if (relativeURL.EndsWith("/"))
                {
                    url = relativeURL + "index.html";
                    if (this.m_filesPool.ContainsEntry(url))
                    {
                        return url;
                    }
                }
                else if (relativeURL.EndsWith("index"))
                {
                    url = relativeURL + ".html";
                    if (this.m_filesPool.ContainsEntry(url))
                    {
                        return url;
                    }
                }
                else
                {
                    url = relativeURL + "/index.html";
                    if (this.m_filesPool.ContainsEntry(url))
                    {
                        return url;
                    }
                }
                return relativeURL;
            });
        }
        private readonly StaticFilesPool m_filesPool=new StaticFilesPool();
        /// <summary>
        /// 提供文件扩展名和MIME类型之间的映射。
        /// </summary>
        public IContentTypeProvider ContentTypeProvider { get; set; }

        /// <summary>
        /// 重新导航
        /// </summary>
        public Func<HttpRequest, Task<string>> NavigateAction { get; set; }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        public Func<HttpContext, Task> ResponseAction { get; set; }

        public StaticFilesPool FilesPool => this.m_filesPool;

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
        /// 设置提供文件扩展名和MIME类型之间的映射。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public void SetContentTypeProvider(IContentTypeProvider provider)
        {
            this.ContentTypeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// 设定重新导航
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public void SetNavigateAction(Func<HttpRequest, Task<string>> func)
        {
            this.NavigateAction = func;
        }

        /// <summary>
        /// 设定重新导航
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public void SetNavigateAction(Func<HttpRequest, string> func)
        {
            this.NavigateAction = (request) => { return Task.FromResult(func(request)); };
        }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public void SetResponseAction(Func<HttpContext, Task> func)
        {
            this.ResponseAction = func;
        }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public void SetResponseAction(Action<HttpContext> action)
        {
            this.ResponseAction = (response) => { action.Invoke(response); return EasyTask.CompletedTask; };
        }
    }
}