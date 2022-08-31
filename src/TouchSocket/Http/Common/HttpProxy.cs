using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http.WebProxy;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http代理
    /// </summary>
    public class HttpProxy
    {
        /// <summary>
        /// 不带基本验证的代理
        /// </summary>
        /// <param name="host"></param>
        public HttpProxy(IPHost host)
        {
            this.Host = host;
        }
        /// <summary>
        /// 带基本验证的代理
        /// </summary>
        /// <param name="host"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        public HttpProxy(IPHost host, string userName, string passWord)
        {
            Host = host;
            Credential = new NetworkCredential(userName, passWord, $"{host.IP}:{host.Port}");
        }

        /// <summary>
        /// 代理的地址
        /// </summary>
        public IPHost Host { get; set; }
        /// <summary>
        /// 验证代理
        /// </summary>
        public NetworkCredential Credential { get; set; }
    }
}
