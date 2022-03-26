//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Log;
using System;
using System.IO;

namespace RRQMSocket.Http.Plugins
{
    /// <summary>
    /// Http静态内容插件
    /// </summary>
    public class HttpStaticPagePlugin : HttpPluginBase
    {
        private FileCachePool fileCache;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpStaticPagePlugin()
        {
            this.fileCache = new FileCachePool();
        }

        /// <summary>
        /// 静态文件缓存。
        /// </summary>
        public FileCachePool FileCache => this.fileCache;

        /// <summary>
        /// 添加静态
        /// </summary>
        /// <param name="path">Static content path</param>
        /// <param name="prefix">Cache prefix (default is "/")</param>
        /// <param name="filter">Cache filter (default is "*.*")</param>
        /// <param name="timeout">Refresh cache timeout (default is 1 hour)</param>
        public void AddFolder(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromHours(1);

            this.fileCache.InsertPath(path, prefix, filter, timeout.Value, null);
        }

        /// <summary>
        /// Clear static content cache
        /// </summary>
        public void ClearFolder()
        {
            this.fileCache.Clear();
        }

        /// <summary>
        /// Remove static content cache
        /// </summary>
        /// <param name="path">Static content path</param>
        public void RemoveFolder(string path)
        {
            this.fileCache.RemovePath(path);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.fileCache.Find(e.Request.RelativeURL, out byte[] data))
            {
                HttpResponse header = new HttpResponse();
                header.StatusCode = "200";
                header.SetContentTypeByExtension(Path.GetExtension(e.Request.RelativeURL).Replace(".", string.Empty));
                header.SetHeader("Cache-Control", $"max-age={TimeSpan.FromSeconds(60)}");
                header.SetContent(data);
                e.Response = header;
                e.Handled = true;
            }
            base.OnGet(client, e);
        }
    }
}
