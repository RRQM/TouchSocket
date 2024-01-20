using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// HttpContainerExtension
    /// </summary>
    public static class HttpContainerExtension
    {
        /// <summary>
        /// 向注册器中添加跨域服务。
        /// </summary>
        /// <param name="registrator"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegistrator AddCors(this IRegistrator registrator, Action<CorsOptions> action)
        {
            var corsOptions = new CorsOptions();
            action.Invoke(corsOptions);
            return registrator.RegisterSingleton<ICorsService>(new CorsService(corsOptions));
        }
    }
}
