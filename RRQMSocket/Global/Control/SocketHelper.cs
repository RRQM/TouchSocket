using System.Net.Sockets;

namespace RRQMSocket.Extension
{
    /// <summary>
    /// RRQMSocket扩展方法
    /// </summary>
    public static class SocketHelper
    {
        /// <summary>
        /// 获取通信器
        /// </summary>
        /// <param name="baseSocket"></param>
        /// <returns></returns>
        public static Socket GetSocket(this BaseSocket baseSocket)
        {
            return baseSocket.MainSocket;
        }
    }
}