using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 标识在线状态的对象
    /// </summary>
    public interface IOnlineClient
    {
        /// <summary>
        /// 判断是否在线
        /// </summary>
        bool Online { get; }
    }
}
