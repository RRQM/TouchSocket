using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 通过ID发送
    /// </summary>
    public interface IIDSender
    {
        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(string id, byte[] buffer, int offset, int length);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(string id, byte[] buffer);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="byteBlock">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(string id, ByteBlock byteBlock);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(string id, byte[] buffer, int offset, int length);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="buffer">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(string id, byte[] buffer);

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="byteBlock">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(string id, ByteBlock byteBlock);
    }
}
