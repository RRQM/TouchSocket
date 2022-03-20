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
using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http响应
    /// </summary>
    public class HttpResponse : HttpBase
    {
        /// <summary>
        /// 状态码，默认200
        /// </summary>
        public string StatusCode { get; set; } = "200";

        /// <summary>
        /// 是否包含分块
        /// </summary>
        public bool HasChunk { get; set; }

        /// <summary>
        /// 分块数据
        /// </summary>
        public List<byte[]> Chunks { get; set; }

        /// <summary>
        /// 状态消息，默认Success
        /// </summary>
        public string StatusMessage { get; set; } = "Success";

        /// <summary>
        /// 构建响应数据
        /// </summary>
        /// <param name="byteBlock"></param>
        public override void Build(ByteBlock byteBlock)
        {
            this.BuildHeader(byteBlock);
            this.BuildContent(byteBlock);
        }


        /// <summary>
        /// 获取头数据
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetHeader(string fieldName)
        {
            return this.GetHeaderByKey(fieldName);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        protected override void LoadHeaderProterties()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0)
            {
                string[] ps = first[0].Split('/');
                if (ps.Length == 2)
                {
                    this.Protocols = ps[0];
                    this.ProtocolVersion = ps[1];
                }
            }
            if (first.Length > 1)
            {
                this.StatusCode = first[1];
            }
            if (first.Length > 2)
            {
                this.StatusMessage = first[2];
            }

            string transferEncoding = this.GetHeader(HttpHeaders.TransferEncoding);
            if ("chunked".Equals(transferEncoding, StringComparison.OrdinalIgnoreCase))
            {
                this.HasChunk = true;
            }
        }

        /// <summary>
        /// 设置头数据
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void SetHeader(HttpHeaders header, string value)
        {
            this.SetHeaderByKey(header, value);
        }

        /// <summary>
        /// 设置头数据
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void SetHeader(string fieldName, string value)
        {
            this.SetHeaderByKey(fieldName, value);
        }

        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.ContentLength > 0)
            {
                byteBlock.Write(this.content);
            }
        }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        private void BuildHeader(ByteBlock byteBlock)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"HTTP/{this.ProtocolVersion} {this.StatusCode} {this.StatusMessage}");

            if (this.ContentLength > 0)
            {
                this.SetHeaderByKey(HttpHeaders.ContentLength, this.ContentLength.ToString());
            }

            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.AppendLine(this.Headers[headerkey]);
            }

            stringBuilder.AppendLine();
            byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override byte[] GetContent()
        {
            if (this.HasChunk)
            {
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    foreach (var item in this.Chunks)
                    {
                        byteBlock.Write(item);
                    }

                    return byteBlock.ToArray();
                }
            }
            else
            {
                return this.content;
            }
        }
        byte[] content;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="content"></param>
        public override void SetContent(byte[] content)
        {
            this.content = content;
            this.ContentLength = content.Length;
        }
    }
}