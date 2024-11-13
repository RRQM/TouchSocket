using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    internal class InternalFormCollection : Dictionary<string, string>, IFormCollection
    {
        private readonly InternalMultifileCollection m_files = new InternalMultifileCollection();

        public InternalFormCollection(ReadOnlyMemory<byte> context, Span<byte> boundary) : this()
        {
            if (context.IsEmpty)
            {
                return;
            }

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

                    if (internalFormFile.FileName.IsNullOrEmpty())
                    {
                        base.Add(internalFormFile.Name, internalFormFile.Data.Span.ToString(Encoding.UTF8));
                    }
                    else
                    {
                        m_files.Add(internalFormFile);
                    }
                }
            }
        }

        public InternalFormCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public InternalFormCollection(ReadOnlyMemory<byte> context)
        {
            var row = context.Span.ToString(Encoding.UTF8);
            if (string.IsNullOrEmpty(row))
            {
                return;
            }

            var kvs = row.Split('&');
            if (kvs == null || kvs.Length == 0)
            {
                return;
            }

            foreach (var item in kvs)
            {
                var kv = item.SplitFirst('=');
                if (kv.Length == 2)
                {
                    this.AddOrUpdate(kv[0], kv[1]);
                }
            }
        }

        int IFormCollection.Count => base.Count;
        public IMultifileCollection Files => this.m_files;

        ICollection<string> IFormCollection.Keys => base.Keys;
        string IFormCollection.this[string key] => base[key];

        string IFormCollection.Get(string key)
        {
            if (this.TryGetValue(key,out var value))
            {
                return value;
            }
            return null;
        }
    }
}