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
using System.Net.Sockets;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 客户端扩展类
    /// </summary>
    public static class ClientExtension
    {
        /// <summary>
        /// 获取相关信息。格式：
        ///<para>IPPort=IP:Port，ID=id，Protocol=Protocol</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetInfo<T>(this T client) where T : ISocketClient
        {
            return $"IP&Port={client.IP}:{client.Port},ID={client.ID},Protocol={client.Protocol}";
        }

        /// <summary>
        /// 安全性发送关闭报文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="how"></param>
        public static void SafeShutdown<T>(this T client, SocketShutdown how = SocketShutdown.Both) where T : ITcpClientBase
        {
            try
            {
                if (client == null || !client.Online)
                {
                    return;
                }
                client.Shutdown(how);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 获取IP和端口。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetIPPort<T>(this T client) where T : ITcpClientBase
        {
            return $"{client.IP}:{client.Port}";
        }

        #region Udp广播
        /// <summary>
        /// 广播。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="port"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static void SendWithBroadcast<T>(this T client, int port, byte[] buffer, int offset, int length) where T : IUdpClientSender
        {
            //client.Send(new IPHost($"255.255.255.255:{port}").EndPoint,buffer,offset,length);
        }
        #endregion
    }
}
