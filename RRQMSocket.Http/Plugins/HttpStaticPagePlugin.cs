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
