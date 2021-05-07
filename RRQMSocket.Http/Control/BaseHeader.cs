//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http基础头部
    /// </summary>
    public class BaseHeader : IDisposable
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public static readonly string ServerVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// 请求体流数据
        /// </summary>
        public ByteBlock Body { get; set; }

        /// <summary>
        /// 请求体字符数据
        /// </summary>
        public string BodyString { get; set; }

        /// <summary>
        /// 编码方式
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string Content_Type { get; set; }

        /// <summary>
        /// 内容长度
        /// </summary>
        public int Content_Length { get; set; }

        /// <summary>
        /// 内容编码
        /// </summary>
        public string Content_Encoding { get; set; }

        /// <summary>
        /// 内容语言
        /// </summary>
        public string ContentLanguage { get; set; }

        /// <summary>
        /// 请求头集合
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// HTTP协议版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 协议名称
        /// </summary>
        public string Protocols { get; set; }

        /// <summary>
        /// 获取头集合的值
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        protected string GetHeaderByKey(Enum header)
        {
            var fieldName = header.GetDescription();
            if (fieldName == null) return null;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return Headers[fieldName];
        }

        /// <summary>
        /// 获取头集合的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected string GetHeaderByKey(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return null;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return Headers[fieldName];
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        protected void SetHeaderByKey(Enum header, string value)
        {
            var fieldName = header.GetDescription();
            if (fieldName == null) return;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) Headers.Add(fieldName, value);
            Headers[fieldName] = value;
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        protected void SetHeaderByKey(string fieldName, string value)
        {
            if (string.IsNullOrEmpty(fieldName)) return;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) Headers.Add(fieldName, value);
            Headers[fieldName] = value;
        }

        /// <summary>
        /// 释放所占资源
        /// </summary>
        public void Dispose()
        {
            if (this.Body != null)
            {
                this.Body.Dispose();
                this.Body = null;
            }
        }
    }
}