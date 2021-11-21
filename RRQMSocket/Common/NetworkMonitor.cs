using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 网络监听器
    /// </summary>
    public class NetworkMonitor
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iPHost"></param>
        /// <param name="socket"></param>
        public NetworkMonitor(IPHost iPHost, Socket socket)
        {
            this.iPHost = iPHost;
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        private IPHost iPHost;

        /// <summary>
        /// 监听地址组
        /// </summary>
        public IPHost IPHost
        {
            get { return iPHost; }
        }

        private Socket socket;

        /// <summary>
        /// Socket组件
        /// </summary>
        public Socket Socket
        {
            get { return socket; }
        }


    }
}
