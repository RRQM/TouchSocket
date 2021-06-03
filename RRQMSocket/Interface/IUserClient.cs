using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Exceptions;

namespace RRQMSocket
{
    /// <summary>
    /// 用户终端接口
    /// </summary>
    public interface IUserClient
    {
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        void Connect();

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <param name="callback"></param>
        /// <exception cref="RRQMException"></exception>
        void ConnectAsync(Action<AsyncResult> callback = null);


        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        void Setup(TcpClientConfig clientConfig);
    }
}
