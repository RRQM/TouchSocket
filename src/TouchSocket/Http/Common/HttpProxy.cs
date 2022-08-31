using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http代理
    /// </summary>
    public class HttpProxy
    {
        /// <summary>
        /// 代理的地址
        /// </summary>
        public IPHost Host { get; set; }
    }
}
