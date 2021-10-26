using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace RRQMCore.Extensions.Tools
{
    /// <summary>
    /// 网络工具
    /// </summary>
    public static class NetTools
    {
        /// <summary>
        /// 获取MAC地址
        /// </summary>
        /// <returns></returns>
        public static string[] GetMacByNetworkInterface()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                List<string> mac = new List<string>();

                foreach (NetworkInterface ni in interfaces)
                {
                    mac.Add(BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()));
                }
                return mac.ToArray();
            }
            catch (Exception)
            {
            }
            return new string[0];
        }
    }
}