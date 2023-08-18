using System;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 用于路由的服务接口
    /// </summary>
    public interface IDmtpRouteService
    {
        /// <summary>
        /// 查找其他IDmtpActor
        /// </summary>
        Func<string, IDmtpActor> FindDmtpActor { get; set; }
    }
}
