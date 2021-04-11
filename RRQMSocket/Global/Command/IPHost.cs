using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// IP解析映射
    /// </summary>
   public class IPHost
    {
        private IPHost()
        { 
        
        }

        /// <summary>
        /// 从字符串获取Host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IPHost CreatIPHost(string host)
        {
            IPHost iPHost = new IPHost();

            int r = host.LastIndexOf(":");
            iPHost.IP =host.Substring(0, r);
            iPHost.Port = Convert.ToInt32(host.Substring(r + 1, host.Length - (r + 1)));
            iPHost.EndPoint = new IPEndPoint(IPAddress.Parse(iPHost.IP),iPHost.Port);
            if (iPHost.IP.Contains(":"))
            {
                iPHost.AddressFamily = AddressFamily.InterNetworkV6;
            }
            else
            {
                iPHost.AddressFamily = AddressFamily.InterNetwork;
            }

            return iPHost;
        }

        /// <summary>
        /// IP
        /// </summary>
        public string IP { get;private set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get;private set; }
        /// <summary>
        /// 寻址方案
        /// </summary>
        public AddressFamily AddressFamily { get;private set; }

        /// <summary>
        /// 终结点
        /// </summary>
        public EndPoint EndPoint { get;private set; }
    }
}
