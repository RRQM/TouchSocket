//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IP解析映射
    /// </summary>
    public class IPHost
    {
        private readonly bool isUri;

        private readonly Uri uri;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">可以输入类似“127.0.0.1:7789”、“http://baidu.com”类型的参数</param>
        public IPHost(string host)
        {
            if (TouchSocketUtility.IsURL(host))
            {
                this.isUri = true;
                this.uri = new Uri(host);
                if (this.uri.Port > 0)
                {
                    this.Host = $"{this.uri.Host}:{this.uri.Port}";
                }
                else
                {
                    this.Host = this.uri.Host;
                }
                if (TouchSocketUtility.IsIPv4(this.uri.Host) || TouchSocketUtility.IsIPV6(this.uri.Host))
                {
                    this.Analysis(this.uri.Host, this.uri.Port.ToString());
                }
                else
                {
                    if (HostNameToIP(this.uri.Host, out IPAddress[] addresses))
                    {
                        this.Analysis(addresses[0].ToString(), this.uri.Port.ToString());
                    }
                }
            }
            else
            {
                this.Host = host;
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
        /// 具有端口信息的host
        /// </summary>
        public string Host { get; private set; }

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
        public bool IsUri => this.isUri;

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 统一资源标识
        /// </summary>
        public Uri Uri => this.uri;

        /// <summary>
        /// 获取Url全路径
        /// </summary>
        /// <returns></returns>
        public string GetUrlPath()
        {
            if (this.isUri)
            {
                return this.uri.PathAndQuery;
            }
            return default;
        }

        /// <summary>
        /// 返回EndPoint字符串
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
            catch (Exception ex)
            {
                throw new Exception($"IPHost初始化失败，信息:{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解析一个组的地址。
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static IPHost[] ParseIPHosts(string[] strs)
        {
            List<IPHost> iPs = new List<IPHost>();
            foreach (var item in strs)
            {
                iPs.Add(new IPHost(item));
            }
            return iPs.ToArray();
        }
    }
}