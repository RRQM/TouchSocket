using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// 用于路由的服务接口
    /// </summary>
    public interface ISmtpRouteService
    {
        /// <summary>
        /// 查找其他ISmtpActor
        /// </summary>
        Func<string, ISmtpActor> FindSmtpActor { get; set; }
    }
}
