using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// 路由服务实现。
    /// </summary>
    public class SmtpRouteService : ISmtpRouteService
    {
        /// <summary>
        /// 查找路由的委托
        /// </summary>
        public Func<string, ISmtpActor> FindSmtpActor { get; set; }
    }
}
