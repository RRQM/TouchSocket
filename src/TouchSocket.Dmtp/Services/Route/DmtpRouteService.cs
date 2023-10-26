using System;
using System.Threading.Tasks;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 路由服务实现。
    /// </summary>
    public class DmtpRouteService : IDmtpRouteService
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<string, Task<IDmtpActor>> FindDmtpActor { get; set; }
    }
}