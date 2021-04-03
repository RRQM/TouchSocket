//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RRQMSocket.RPC
{
    /*
    若汝棋茗
    */

    /// <summary>
    /// Http请求报文格式
    /// </summary>
    public class HttpRequest : BaseHeader
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="httpString"></param>
        public HttpRequest(string httpString)
        {
            string[] rows = Regex.Split(httpString, Environment.NewLine);

            //Request URL & Method & Version
            var first = Regex.Split(rows[0], @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0) this.Method = first[0];
            if (first.Length > 1) this.URL = Uri.UnescapeDataString(first[1]);
            if (first.Length > 2) this.ProtocolVersion = first[2];

            //Request Headers
            this.Headers = GetRequestHeaders(rows);

            //Request Body
            this.Body = GetRequestBody(rows);
            var contentLength = this.GetHeaderByKey(RequestHeaders.ContentLength);

            //if (int.TryParse(contentLength, out var length) && Body.Length != length)
            //{
            //    do
            //    {
            //        length = stream.Read(bytes, 0, MAX_SIZE - 1);
            //        Body += Encoding.UTF8.GetString(bytes, 0, length);
            //    } while (Body.Length != length);
            //}

            //Request "GET"
            if (this.Method == "GET")
            {
                var isUrlencoded = this.URL.Contains('?');
                if (isUrlencoded) this.Params = GetRequestParameters(URL.Split('?')[1]);
            }

            //Request "POST"
            if (this.Method == "POST")
            {
                var contentType = this.GetHeaderByKey(RequestHeaders.ContentType);
                var isUrlencoded = contentType == @"application/x-www-form-urlencoded";
                if (isUrlencoded) this.Params = GetRequestParameters(this.Body);
            }
        }

        /// <summary>
        /// URL参数
        /// </summary>
        public Dictionary<string, string> Params { get; private set; }

        /// <summary>
        /// HTTP请求方式
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// HTTP(S)地址
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// HTTP协议版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        private string GetRequestBody(IEnumerable<string> rows)
        {
            var target = rows.Select((v, i) => new { Value = v, Index = i }).FirstOrDefault(e => e.Value.Trim() == string.Empty);
            if (target == null) return null;
            var range = Enumerable.Range(target.Index + 1, rows.Count() - target.Index - 1);
            return string.Join(Environment.NewLine, range.Select(e => rows.ElementAt(e)).ToArray());
        }

        private Dictionary<string, string> GetRequestHeaders(IEnumerable<string> rows)
        {
            if (rows == null || rows.Count() <= 0) return null;
            var target = rows.Select((v, i) => new { Value = v, Index = i }).FirstOrDefault(e => e.Value.Trim() == string.Empty);
            var length = target == null ? rows.Count() - 1 : target.Index;
            if (length <= 1) return null;
            var range = Enumerable.Range(1, length - 1);
            return range.Select(e => rows.ElementAt(e)).ToDictionary(e => e.Split(':')[0], e => e.Split(':')[1].Trim());
        }

        private Dictionary<string, string> GetRequestParameters(string row)
        {
            if (string.IsNullOrEmpty(row)) return null;
            var kvs = Regex.Split(row, "&");
            if (kvs == null || kvs.Count() <= 0) return null;

            return kvs.ToDictionary(e => Regex.Split(e, "=")[0], e => Regex.Split(e, "=")[1]);
        }
    }
}