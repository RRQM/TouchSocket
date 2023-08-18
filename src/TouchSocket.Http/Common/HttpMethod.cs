using System;

namespace TouchSocket.Http
{
    /// <summary>
    /// HttpMethod
    /// </summary>
    public readonly struct HttpMethod
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

        /// <summary>
        /// 比较是否和目标相等
        /// </summary>
        /// <param name="obj">目标</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is HttpMethod && this.GetHashCode() == obj.GetHashCode();
        }

        /// <summary>
        /// 等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(HttpMethod a, HttpMethod b)
        {
            return string.IsNullOrEmpty(a.m_value) && string.IsNullOrEmpty(b.m_value) || string.Equals(a.m_value, b.m_value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 不等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(HttpMethod a, HttpMethod b)
        {
            var state = a == b;
            return !state;
        }
    }
}
