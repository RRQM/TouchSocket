using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// 可以配置跨域的插件
    /// </summary>
    public class CorsPlugin : PluginBase
    {
        /// <summary>
        /// 允许客户端携带验证信息
        /// </summary>
        public bool Credentials { get; set; }

        /// <summary>
        /// 允许跨域的方法。
        /// 默认为“PUT,POST,GET,DELETE,OPTIONS,HEAD,PATCH”
        /// </summary>
        public string Methods { get; set; }

        /// <summary>
        /// 允许跨域的域名
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// 允许任何Method。实际上包含：PUT,POST,GET,DELETE,OPTIONS,HEAD,PATCH
        /// </summary>
        /// <returns></returns>
        public CorsPlugin AllowAnyMethod()
        {
            this.Methods = "PUT,POST,GET,DELETE,OPTIONS,HEAD,PATCH";
            return this;
        }

        /// <summary>
        /// 允许所有的源
        /// </summary>
        /// <returns></returns>
        public CorsPlugin AllowAnyOrigin()
        {
            this.Origin = "*";
            return this;
        }

        /// <summary>
        /// 允许客户端携带验证信息
        /// </summary>
        /// <returns></returns>
        public CorsPlugin AllowCredentials()
        {
            this.Credentials = true;
            return this;
        }

        /// <summary>
        /// 允许跨域的方法。
        /// 例如“PUT,POST,GET,DELETE,OPTIONS,HEAD,PATCH”
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public CorsPlugin SetMethods(string value)
        {
            this.Methods = value;
            return this;
        }

        /// <summary>
        /// 允许跨域的域名
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public CorsPlugin SetOrigin(string value)
        {
            this.Origin = value;
            return this;
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            pluginManager.Add<IHttpSocketClient, HttpContextEventArgs>(nameof(IHttpPlugin.OnHttpRequest), OnHttpRequest);
            base.Loaded(pluginManager);
        }

        private async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.Credentials)
            {
                e.Context.Response.Headers.Add("Access-Control-Allow-Credentials", this.Credentials.ToString().ToLower());
            }
            if (this.Methods.HasValue())
            {
                e.Context.Response.Headers.Add("Access-Control-Allow-Methods", this.Methods);
            }
            if (this.Origin.HasValue())
            {
                e.Context.Response.Headers.Add("Access-Control-Allow-Origin", this.Origin);
            }

            await e.InvokeNext();
        }
    }
}