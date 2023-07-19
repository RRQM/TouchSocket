using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 路由服务实现。
    /// </summary>
    public class DmtpRouteService : IDmtpRouteService
    {
        /// <summary>
        /// 查找路由的委托
        /// </summary>
        public Func<string, IDmtpActor> FindDmtpActor { get; set; }
    }
}
