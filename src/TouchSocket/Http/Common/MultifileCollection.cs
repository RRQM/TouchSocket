using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TouchSocket.Core.Extensions;

namespace TouchSocket.Http
{
    /// <summary>
    /// 多文件集合
    /// </summary>
    public class MultifileCollection : IEnumerable<IFormFile>
    {
        private readonly HttpRequest m_httpRequest;

        /// <summary>
        /// 多文件集合
        /// </summary>
        /// <param name="httpRequest"></param>
        public MultifileCollection(HttpRequest httpRequest)
        {
            this.m_httpRequest = httpRequest;
        }

        /// <summary>
        /// 获取一个迭代器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IFormFile> GetEnumerator()
        {
            if (this.m_httpRequest.ContentComplated == null || this.m_httpRequest.ContentComplated == true)
            {
                if (this.m_httpRequest.TryGetContent(out byte[] context))
                {
                    byte[] boundary = $"--{this.m_httpRequest.GetBoundary()}".ToUTF8Bytes();
                    var indexs = context.IndexOfInclude(0, context.Length, boundary);
                    if (indexs.Count <= 0)
                    {
                        throw new Exception("没有发现由Boundary包裹的数据。");
                    }
                    List<IFormFile> files = new List<IFormFile>();
                    for (int i = 0; i < indexs.Count; i++)
                    {
                        if (i + 1 < indexs.Count)
                        {
                            InternalFormFile internalFormFile = new InternalFormFile();
                            files.Add(internalFormFile);
                            int index = context.IndexOfFirst(indexs[i] + 3, indexs[i + 1], Encoding.UTF8.GetBytes("\r\n\r\n"));
                            string line = Encoding.UTF8.GetString(context, indexs[i] + 3, index - indexs[i] - 6);
                            string[] lines = line.Split(new string[] { ";", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            internalFormFile.DataPair = new NameValueCollection();
                            foreach (var item in lines)
                            {
                                string[] kv = item.Split(new char[] { ':', '=' });
                                if (kv.Length == 2)
                                {
                                    internalFormFile.DataPair.Add(kv[0].Trim(), kv[1].Replace("\"",String.Empty).Trim());
                                }
                            }

                            int length = indexs[i + 1] - (index + 2) - boundary.Length;
                            byte[] data = new byte[length];
                            Array.Copy(context, index + 1, data, 0, length);
                            //string ssss = Encoding.UTF8.GetString(data);
                            internalFormFile.Data = data;
                        }
                    }

                    return files.GetEnumerator();
                }
                throw new Exception("管道状态异常");
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
}