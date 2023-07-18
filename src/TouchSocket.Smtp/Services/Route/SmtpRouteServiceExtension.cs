using System;
using TouchSocket.Core;
using TouchSocket.Smtp;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// SmtpRouteServiceExtension
    /// </summary>
    public static class SmtpRouteServiceExtension
    {
        /// <summary>
        /// 添加基于Tcp服务器的SMTP路由服务。
        /// </summary>
        /// <param name="container"></param>
        public static void AddSmtpRouteService(this IContainer container)
        {
            container.RegisterSingleton<ISmtpRouteService, SmtpRouteService>();
        }

        /// <summary>
        /// 添加基于设定委托的SMTP路由服务。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="func"></param>
        public static void AddSmtpRouteService(this IContainer container, Func<string, ISmtpActor> func)
        {
            container.RegisterSingleton<ISmtpRouteService>(new SmtpRouteService()
            {
                FindSmtpActor = func
            });
        }
    }
}
