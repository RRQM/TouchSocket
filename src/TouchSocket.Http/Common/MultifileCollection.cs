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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// 多文件集合
    /// </summary>
    public partial class MultifileCollection : IEnumerable<IFormFile>
    {
        private readonly HttpRequest m_request;

        /// <summary>
        /// 多文件集合
        /// </summary>
        /// <param name="request"></param>
        public MultifileCollection(HttpRequest request)
        {
            this.m_request = request;
        }

        /// <summary>
        /// 获取一个迭代器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IFormFile> GetEnumerator()
        {
            if (this.m_request.ContentCompleted == null || this.m_request.ContentCompleted == true)
            {
                var context = this.m_request.GetContent(CancellationToken.None);

                var boundary = $"--{this.m_request.GetBoundary()}".ToUTF8Bytes();


                var indexes = context.Span.IndexOfInclude(0, context.Length, boundary);
                if (indexes.Count <= 0)
                {
                    throw new Exception("没有发现由Boundary包裹的数据。");
                }
                //var files = new List<IFormFile>();
                for (var i = 0; i < indexes.Count; i++)
                {
                    if (i + 1 < indexes.Count)
                    {
                        var internalFormFile = new InternalFormFile();
                        //files.Add(internalFormFile);
                        var index = context.Span.IndexOfFirst(indexes[i] + 3, indexes[i + 1], Encoding.UTF8.GetBytes("\r\n\r\n"));
                        var line = context.Span.Slice(indexes[i] + 3, index - indexes[i] - 6).ToString(Encoding.UTF8);
                        var lines = line.Split(new string[] { ";", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        internalFormFile.DataPair = new NameValueCollection();
                        foreach (var item in lines)
                        {
                            var kv = item.Split(new char[] { ':', '=' });
                            if (kv.Length == 2)
                            {
                                internalFormFile.DataPair.Add(kv[0].Trim(), kv[1].Replace("\"", String.Empty).Trim());
                            }
                        }

                        var length = indexes[i + 1] - (index + 2) - boundary.Length;
                        //var data = new byte[length];
                        //Array.Copy(context, index + 1, data, 0, length);
                        //string ssss = Encoding.UTF8.GetString(data);
                        internalFormFile.Data = context.Slice(index + 1, length);
                        yield return internalFormFile;
                    }
                }

                //return files.GetEnumerator();
            }
            else
            {
                throw new Exception("管道状态异常");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

#if AsyncEnumerable

    public partial class MultifileCollection : IAsyncEnumerable<IFormFile>
    {
        /// <inheritdoc/>
        public async IAsyncEnumerator<IFormFile> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (this.m_request.ContentCompleted == null || this.m_request.ContentCompleted == true)
            {
                var context = await this.m_request.GetContentAsync(CancellationToken.None).ConfigureAwait(false);

                var boundary = $"--{this.m_request.GetBoundary()}".ToUTF8Bytes();


                var indexes = context.Span.IndexOfInclude(0, context.Length, boundary);
                if (indexes.Count <= 0)
                {
                    throw new Exception("没有发现由Boundary包裹的数据。");
                }
                //var files = new List<IFormFile>();
                for (var i = 0; i < indexes.Count; i++)
                {
                    if (i + 1 < indexes.Count)
                    {
                        var internalFormFile = new InternalFormFile();
                        //files.Add(internalFormFile);
                        var index = context.Span.IndexOfFirst(indexes[i] + 3, indexes[i + 1], Encoding.UTF8.GetBytes("\r\n\r\n"));
                        var line = context.Span.Slice(indexes[i] + 3, index - indexes[i] - 6).ToString(Encoding.UTF8);
                        var lines = line.Split(new string[] { ";", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        internalFormFile.DataPair = new NameValueCollection();
                        foreach (var item in lines)
                        {
                            var kv = item.Split(new char[] { ':', '=' });
                            if (kv.Length == 2)
                            {
                                internalFormFile.DataPair.Add(kv[0].Trim(), kv[1].Replace("\"", String.Empty).Trim());
                            }
                        }

                        var length = indexes[i + 1] - (index + 2) - boundary.Length;
                        //var data = new byte[length];
                        //Array.Copy(context, index + 1, data, 0, length);
                        //string ssss = Encoding.UTF8.GetString(data);
                        internalFormFile.Data = context.Slice(index + 1, length);
                        yield return internalFormFile;
                    }
                }

                //return files.GetEnumerator();
            }
            else
            {
                throw new Exception("管道状态异常");
            }
        }
    }

#endif
}