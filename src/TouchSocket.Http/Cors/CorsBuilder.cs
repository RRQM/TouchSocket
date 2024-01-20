using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// CorsBuilder
    /// </summary>
    public class CorsBuilder
    {
        /// <summary>
        /// 允许客户端携带验证信息
        /// </summary>
        public bool Credentials { get; private set; }

        /// <summary>
        /// 请求头
        /// </summary>
        public string Headers { get; private set; }

        /// <summary>
        /// 允许跨域的方法。
        /// </summary>
        public string Methods { get; private set; }

        /// <summary>
        /// 允许跨域的域名
        /// </summary>
        public string Origin { get; private set; }

        /// <summary>
        /// 允许所有的Header
        /// </summary>
        /// <returns></returns>
        public CorsBuilder AllowAnyHeaders()
        {
            this.Headers = "*";
            return this;
        }

        /// <summary>
        /// 允许任何Method。
        /// </summary>
        /// <returns></returns>
        public CorsBuilder AllowAnyMethod()
        {
            this.Methods = "*";
            return this;
        }

        /// <summary>
        /// 允许所有的源
        /// </summary>
        /// <returns></returns>
        public CorsBuilder AllowAnyOrigin()
        {
            this.Origin = "*";
            return this;
        }

        /// <summary>
        /// 允许客户端携带验证信息
        /// </summary>
        /// <returns></returns>
        public CorsBuilder AllowCredentials()
        {
            this.Credentials = true;
            return this;
        }

        /// <summary>
        /// 允许跨域的Header
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public CorsBuilder WithHeaders(params string[] values)
        {
            this.Headers = string.Join(",", values);
            return this;
        }

        /// <summary>
        /// 允许跨域的方法。
        /// 例如“PUT,POST,GET,DELETE,OPTIONS,HEAD,PATCH”
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public CorsBuilder WithMethods(params string[] values)
        {
            this.Methods = string.Join(",", values);
            return this;
        }

        /// <summary>
        /// 允许跨域的域名
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public CorsBuilder WithOrigin(string value)
        {
            this.Origin = value;
            return this;
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        public CorsPolicy Build()
        {
            return new CorsPolicy(this.Credentials,this.Headers,this.Methods,this.Origin);
        }
    }
}
