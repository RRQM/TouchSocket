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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// 静态页面配置
    /// </summary>
    public class StaticPageOptions
    {
        /// <summary>
        /// 构造函数：初始化StaticPageOptions实例
        /// </summary>
        public StaticPageOptions()
        {
            // 设置导航动作，用于处理静态页面的访问逻辑
            this.SetNavigateAction(request =>
            {
                // 获取请求的相对URL
                var relativeURL = request.RelativeURL;
                // 初始化URL变量为相对URL
                var url = relativeURL;

                // 检查文件池中是否包含直接对应的URL
                if (this.m_filesPool.ContainsEntry(url))
                {
                    // 如果存在直接返回该URL
                    return url;
                }

                // 检查相对URL是否以"/"结尾
                if (relativeURL.EndsWith("/"))
                {
                    // 尝试拼接成index.html的URL
                    url = relativeURL + "index.html";
                    // 检查文件池中是否存在该URL
                    if (this.m_filesPool.ContainsEntry(url))
                    {
                        // 如果存在返回该URL
                        return url;
                    }
                }
                // 检查相对URL是否以"index"结尾
                else if (relativeURL.EndsWith("index"))
                {
                    // 尝试拼接成".html"的URL
                    url = relativeURL + ".html";
                    // 检查文件池中是否存在该URL
                    if (this.m_filesPool.ContainsEntry(url))
                    {
                        // 如果存在返回该URL
                        return url;
                    }
                }
                // 如果以上条件都不满足
                else
                {
                    // 尝试拼接成"/index.html"的URL
                    url = relativeURL + "/index.html";
                    // 检查文件池中是否存在该URL
                    if (this.m_filesPool.ContainsEntry(url))
                    {
                        // 如果存在返回该URL
                        return url;
                    }
                }
                // 如果所有尝试都未能找到对应的文件，则返回原始的相对URL
                return relativeURL;
            });
        }
        private readonly StaticFilesPool m_filesPool = new StaticFilesPool();
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

        /// <summary>
        /// 获取静态文件池对象
        /// </summary>
        public StaticFilesPool FilesPool => this.m_filesPool;

        /// <summary>
        /// 添加静态文件目录
        /// </summary>
        /// <param name="path">静态内容路径</param>
        /// <param name="prefix">缓存前缀（默认为"/"）</param>
        /// <param name="filter">缓存过滤器（默认为"*.*"，表示所有文件）</param>
        /// <param name="timeout">刷新缓存的时间间隔（以毫秒为单位，默认为1小时）</param>
        public void AddFolder(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = default)
        {
            // 调用底层方法添加静态文件目录到缓存中
            this.m_filesPool.AddFolder(path, prefix, filter, timeout);
        }

        /// <summary>
        /// 设置提供文件扩展名和MIME类型之间的映射。
        /// </summary>
        /// <param name="provider">一个实现了IContentTypeProvider接口的对象，用于提供文件扩展名与MIME类型的映射。</param>
        public void SetContentTypeProvider(IContentTypeProvider provider)
        {
            // 校验provider参数是否为空，如果为空则抛出ArgumentNullException异常。
            // 这里是确保ContentTypeProvider的设置必须是有效的，非空对象。
            this.ContentTypeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// 设定重新导航
        /// </summary>
        /// <param name="func">一个函数，接受HttpRequest作为参数，并返回一个异步Task，该Task结果为字符串类型的导航目标</param>
        public void SetNavigateAction(Func<HttpRequest, Task<string>> func)
        {
            this.NavigateAction = func;
        }
        /// <summary>
        /// 设定重新导航
        /// </summary>
        /// <param name="func">一个函数，接受一个HttpRequest参数，并返回一个字符串类型的URL</param>
        public void SetNavigateAction(Func<HttpRequest, string> func)
        {
            // 将导航行为设置为一个异步操作，该操作接收一个HttpRequest并返回一个Task<string>类型的结果
            // 这里使用了Lambda表达式来简化代码，并提高可读性
            this.NavigateAction = (request) => { return Task.FromResult(func(request)); };
        }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        /// <param name="func">一个委托，包含在响应之前需要执行的操作。该委托接受一个HttpContext参数，返回一个Task对象。</param>
        public void SetResponseAction(Func<HttpContext, Task> func)
        {
            this.ResponseAction = func; // 设置内部响应操作委托
        }

        /// <summary>
        /// 设置一个操作，该操作将在生成响应之前被调用。
        /// </summary>
        /// <param name="action">一个接受HttpContext作为参数的Action委托，表示要设置的操作。</param>
        public void SetResponseAction(Action<HttpContext> action)
        {
            // 将提供的操作委托赋值给ResponseAction属性。
            // 通过创建一个匿名函数，该函数接受一个HttpContext参数（response），
            // 调用传入的操作委托并返回一个已完成的任务，以此实现对响应操作的设置。
            this.ResponseAction = (response) => { action.Invoke(response); return EasyTask.CompletedTask; };
        }
    }
}