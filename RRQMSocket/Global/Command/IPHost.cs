//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
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
