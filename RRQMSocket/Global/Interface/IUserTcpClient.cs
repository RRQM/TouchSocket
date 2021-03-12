using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Exceptions;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端终端接口
    /// </summary>
    public interface IUserTcpClient:IUserClient
    {
        /// <summary>
        /// 连接客户端
        /// </summary>
        /// <param name="setting"></param>
        /// <exception cref="RRQMException"></exception>
        void Connect(ConnectSetting setting);

        /// <summary>
        /// 连接客户端
        /// </summary>
        /// <param name="endPoint"></param>
        /// <exception cref="RRQMException"></exception>
        void Connect(EndPoint endPoint);
    }
}
