//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using RRQMCore;
using RRQMSocket.Common;
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
        private bool isUri;

        private Uri uri;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">可以输入类似“127.0.0.1:7789”、“http://baidu.com”类型的参数</param>
        public IPHost(string host)
        {
            if (RRQMSocketTools.IsURL(host))
            {
                this.isUri = true;
                this.uri = new Uri(host);

                if (RRQMSocketTools.IsIPv4(uri.Host) || RRQMSocketTools.IsIPV6(uri.Host))
                {
                    this.Analysis(uri.Host, uri.Port.ToString());
                }
                else
                {
                    if (HostNameToIP(uri.Host, out IPAddress[] addresses))
                    {
                        this.Analysis(addresses[0].ToString(), uri.Port.ToString());
                    }
                }
            }
            else
            {
                int r = host.LastIndexOf(":");
                string ip = host.Substring(0, r);
                this.Analysis(ip, host.Substring(r + 1, host.Length - (r + 1)));
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
        /// 寻址方案
        /// </summary>
        public AddressFamily AddressFamily { get; private set; }

        /// <summary>
        /// 终结点
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 是否为Uri
        /// </summary>
        public bool IsUri
        {
            get { return isUri; }
        }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 统一资源标识
        /// </summary>
        public Uri Uri
        {
            get { return uri; }
        }

        /// <summary>
        /// 返回对象字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.EndPoint == null ? null : this.EndPoint.ToString();
        }

        private static bool HostNameToIP(string hostname, out IPAddress[] address)
        {
            try
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
                address = hostInfo.AddressList;
                if (address.Length > 0)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                address = null;
                return false;
            }
        }

        private void Analysis(string ip, string port)
        {
            try
            {
                int portNum = int.Parse(port);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), portNum);
                if (ip.Contains(":"))
                {
                    this.AddressFamily = AddressFamily.InterNetworkV6;
                }
                else
                {
                    this.AddressFamily = AddressFamily.InterNetwork;
                }
                this.EndPoint = endPoint;
                this.IP = ip;
                this.Port = portNum;
            }
            catch
            {
                throw new RRQMException("IPHost不合法");
            }
        }
    }
}