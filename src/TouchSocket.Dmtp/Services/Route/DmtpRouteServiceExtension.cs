using System;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpRouteServiceExtension
    /// </summary>
    public static class DmtpRouteServiceExtension
    {
        /// <summary>
        /// 添加Dmtp路由服务。
        /// </summary>
        /// <param name="container"></param>
        public static void AddDmtpRouteService(this IContainer container)
        {
            container.RegisterSingleton<IDmtpRouteService, DmtpRouteService>();
        }

        /// <summary>
        /// 添加基于设定委托的Dmtp路由服务。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="func"></param>
        public static void AddDmtpRouteService(this IContainer container, Func<string, IDmtpActor> func)
        {
            container.RegisterSingleton<IDmtpRouteService>(new DmtpRouteService()
            {
                FindDmtpActor = func
            });
        }
    }
}
