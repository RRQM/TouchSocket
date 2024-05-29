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
using System.Diagnostics;

namespace TouchSocket.Http
{
    /// <summary>
    /// HttpMethod
    /// </summary>
    [DebuggerDisplay("{m_value}")]
    public readonly record struct HttpMethod
    {
        /// <summary>
        /// 值
        /// </summary>
        private readonly string m_value;

        /// <summary>
        /// Get
        /// </summary>
        public static readonly HttpMethod Get = new HttpMethod("get");

        /// <summary>
        /// Post
        /// </summary>
        public static readonly HttpMethod Post = new HttpMethod("post");

        /// <summary>
        /// Put
        /// </summary>
        public static readonly HttpMethod Put = new HttpMethod("put");

        /// <summary>
        /// Delete
        /// </summary>
        public static readonly HttpMethod Delete = new HttpMethod("delete");

        /// <summary>
        /// 表示
        /// </summary>
        /// <param name="value">值</param>
        public HttpMethod(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException();
            }
            this.m_value = value.ToUpper();
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.m_value;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.m_value.GetHashCode();
        }
    }
}