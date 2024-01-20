using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// CorsResult
    /// </summary>
    public class CorsPolicy
    {
        /// <summary>
        /// CorsResult
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="headers"></param>
        /// <param name="methods"></param>
        /// <param name="origin"></param>
        public CorsPolicy(bool credentials, string headers, string methods, string origin)
        {
            this.Credentials = credentials;
            this.Headers = headers;
            this.Methods = methods;
            this.Origin = origin;
        }

        /// <summary>
        /// 允许客户端携带验证信息
        /// </summary>
        public bool Credentials { get; }

        /// <summary>
        /// 请求头
        /// </summary>
        public string Headers { get; }

        /// <summary>
        /// 允许跨域的方法。
        /// </summary>
        public string Methods { get; }

        /// <summary>
        /// 允许跨域的域名
        /// </summary>
        public string Origin { get; }

        /// <summary>
        /// 应用跨域策略
        /// </summary>
        /// <param name="context"></param>
        public void Apply(HttpContext context)
        {
            if (this.Origin.HasValue())
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", this.Origin);
            }

            if (this.Credentials)
            {
                context.Response.Headers.Add("Access-Control-Allow-Credentials", this.Credentials.ToString().ToLower());
            }

            if (this.Headers.HasValue())
            {
                context.Response.Headers.Add("Access-Control-Allow-Headers", this.Headers);
            }

            if (this.Methods.HasValue())
            {
                context.Response.Headers.Add("Access-Control-Allow-Methods", this.Methods);
            }
        }
    }
}