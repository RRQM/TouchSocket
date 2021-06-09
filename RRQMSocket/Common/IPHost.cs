//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// IP解析映射
    /// </summary>
    public class IPHost
    {
        /// <summary>
        /// 从字符串获取ip和port
        /// </summary>
        public IPHost(string ipHost)
        {
            int r = ipHost.LastIndexOf(":");
            this.IP = ipHost.Substring(0, r);
            this.Port = Convert.ToInt32(ipHost.Substring(r + 1, ipHost.Length - (r + 1)));
            this.EndPoint = new IPEndPoint(IPAddress.Parse(this.IP), this.Port);
            if (this.IP.Contains(":"))
            {
                this.AddressFamily = AddressFamily.InterNetworkV6;
            }
            else
            {
                this.AddressFamily = AddressFamily.InterNetwork;
            }
        }

        /// <summary>
        /// 从IPAddress和端口号
        /// </summary>
        /// <param name="iPAddress"></param>
        /// <param name="port"></param>
        public IPHost(IPAddress iPAddress, int port) : this($"{iPAddress}:{port}")
        {
        }

        /// <summary>
        /// 从端口号创建
        /// </summary>
        /// <param name="port"></param>
        public IPHost(int port) : this($"0.0.0.0:{port}")
        {
        }

        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 寻址方案
        /// </summary>
        public AddressFamily AddressFamily { get; private set; }

        /// <summary>
        /// 终结点
        /// </summary>
        public EndPoint EndPoint { get; private set; }

        /// <summary>
        /// 返回对象字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return EndPoint == null ? null : EndPoint.ToString();
        }
    }
}