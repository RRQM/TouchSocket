using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 客户端发送接口
    /// </summary>
    public interface IClientSender : ISend
    {
        /// <summary>
        /// 同步组合发送数据。
        /// <para>内部已经封装Ssl和发送长度检测，即：调用完成即表示数据全部发送完毕。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="transferBytes">组合数据</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(IList<TransferByte> transferBytes);

        /// <summary>
        /// 异步组合发送数据。
        /// <para>在<see cref="ITcpClient"/>时，如果使用独立线程发送，则不会触发异常。</para>
        /// <para>在<see cref="ITcpClientBase"/>时，相当于<see cref="Socket.BeginSend(byte[], int, int, SocketFlags, out SocketError, System.AsyncCallback, object)"/>。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="transferBytes">组合数据</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(IList<TransferByte> transferBytes);
    }

}
