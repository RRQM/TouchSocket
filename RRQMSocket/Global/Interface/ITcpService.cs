using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP系列服务器接口
    /// </summary>
    public interface ITcpService : IService
    {
        /// <summary>
        /// 获取或设置最大可连接数
        /// </summary>
        int MaxCount { get; set; }

        /// <summary>
        /// 检验客户端活性（避免异常而导致的失活）
        /// </summary>
        bool IsCheckClientAlive { get; set; }

        /// <summary>
        /// 客户端成功连接时
        /// </summary>
        event RRQMMessageEventHandler ClientConnected;

        /// <summary>
        /// 有用户断开连接的时候
        /// </summary>
        event RRQMMessageEventHandler ClientDisconnected;
    }
}
