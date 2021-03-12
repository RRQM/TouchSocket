using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC服务器接口
    /// </summary>
    public interface IRPCService
    {
        /// <summary>
        /// 打开RPC服务器
        /// </summary>
        /// <param name="setting">RPC设置</param>
        void OpenRPCServer(RPCServerSetting setting);

        /// <summary>
        /// 通过IDToken获得实例
        /// </summary>
        /// <param name="iDToken"></param>
        /// <returns></returns>
        ISocketClient GetSocketClient(string iDToken);
    }
}
