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
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket.RPC
{
    /*
    若汝棋茗
    */

    /// <summary>
    ///
    /// </summary>
    public class BaseHeader
    {
        /// <summary>
        ///
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Content_Type { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Content_Length { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Content_Encoding { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ContentLanguage { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 获取键值头
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        protected string GetHeaderByKey(RequestHeaders header)
        {
            if (Headers.ContainsKey(header.ToString()))
            {
                return Headers[header.ToString()];
            }

            return null;
        }

        /// <summary>
        /// 获取键值头
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected string GetHeaderByKey(string fieldName)
        {
            if (Headers.ContainsKey(fieldName))
            {
                return Headers[fieldName];
            }

            return null;
        }

        /// <summary>
        /// 设置键值头
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        protected void SetHeaderByKey(RequestHeaders header, string value)
        {
            if (this.Headers.ContainsKey(header.ToString()))
            {
                this.Headers[header.ToString()] = value;
            }
            else
            {
                this.Headers.Add(header.ToString(), value);
            }
        }

        /// <summary>
        /// 设置键值头
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        protected void SetHeaderByKey(string fieldName, string value)
        {
            if (this.Headers.ContainsKey(fieldName))
            {
                this.Headers[fieldName] = value;
            }
            else
            {
                this.Headers.Add(fieldName, value);
            }
        }
    }
}