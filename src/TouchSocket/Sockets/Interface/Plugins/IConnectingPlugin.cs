using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有预备连接的插件接口
    /// </summary>
    public interface IConnectingPlugin:IPlugin
    {
        /// <summary>
        ///在即将完成连接时触发。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        [AsyncRaiser]
        void OnConnecting(object client, OperationEventArgs e);

        /// <summary>
        /// 在即将完成连接时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnConnectingAsync(object client, OperationEventArgs e);
    }
}
