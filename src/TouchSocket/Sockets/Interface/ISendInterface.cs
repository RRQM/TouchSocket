//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;

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

    /// <summary>
    /// 具有Udp终结点的发送
    /// </summary>
    public interface IUdpClientSender : ISend
    {
        /// <summary>
        /// 同步组合发送数据。
        /// <para>内部已经封装Ssl和发送长度检测，即：调用完成即表示数据全部发送完毕。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="endPoint">远程终结点</param>
        /// <param name="transferBytes">组合数据</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(EndPoint endPoint, IList<TransferByte> transferBytes);

        /// <summary>
        /// 异步组合发送数据。
        /// <para>在<see cref="ITcpClient"/>时，如果使用独立线程发送，则不会触发异常。</para>
        /// <para>在<see cref="ITcpClientBase"/>时，相当于<see cref="Socket.BeginSend(byte[], int, int, SocketFlags, out SocketError, System.AsyncCallback, object)"/>。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="endPoint">远程终结点</param>
        /// <param name="transferBytes">组合数据</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(EndPoint endPoint, IList<TransferByte> transferBytes);
    }

    /// <summary>
    /// 具有直接发送功能
    /// </summary>
    public interface IDefaultSender : ISendBase
    {
        #region 默认发送
        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSend(byte[] buffer, int offset, int length);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSend(byte[] buffer);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSend(ByteBlock byteBlock);
        #endregion

        #region 默认发送
        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSendAsync(byte[] buffer, int offset, int length);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSendAsync(byte[] buffer);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSendAsync(ByteBlock byteBlock);
        #endregion
    }

    /// <summary>
    /// 具有直接发送功能
    /// </summary>
    public interface IUdpDefaultSender : ISendBase
    {
        #region 默认发送
        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="endPoint">目的终结点</param>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSend(EndPoint endPoint, byte[] buffer, int offset, int length);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="endPoint">目的终结点</param>
        /// <param name="buffer">数据缓存区</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSend(EndPoint endPoint, byte[] buffer);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="endPoint">目的终结点</param>
        /// <param name="byteBlock">数据块载体</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSend(EndPoint endPoint, ByteBlock byteBlock);
        #endregion

        #region 默认发送
        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="endPoint">目的终结点</param>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSendAsync(EndPoint endPoint, byte[] buffer, int offset, int length);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="endPoint">目的终结点</param>
        /// <param name="buffer">数据缓存区</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSendAsync(EndPoint endPoint, byte[] buffer);

        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="endPoint">目的终结点</param>
        /// <param name="byteBlock">数据块载体</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSendAsync(EndPoint endPoint, ByteBlock byteBlock);
        #endregion
    }

    /// <summary>
    /// 具有发送动作的基类。
    /// </summary>
    public interface ISendBase
    {
        /// <summary>
        /// 表示对象能否顺利执行发送操作。
        /// <para>由于高并发，当改值为Tru时，也不一定完全能执行。</para>
        /// </summary>
        bool CanSend { get; }
    }

    /// <summary>
    /// 具有发送功能的接口
    /// </summary>
    public interface ISend : ISendBase
    {
        /// <summary>
        /// 同步发送数据。
        /// <para>内部已经封装Ssl和发送长度检测，即：调用完成即表示数据全部发送完毕。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(byte[] buffer, int offset, int length);

        /// <summary>
        /// 同步发送数据。
        /// <para>内部已经封装Ssl和发送长度检测，即：调用完成即表示数据全部发送完毕。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(byte[] buffer);

        /// <summary>
        /// 同步发送数据。
        /// <para>内部已经封装Ssl和发送长度检测，即：调用完成即表示数据全部发送完毕。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="byteBlock">数据块</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(ByteBlock byteBlock);

        /// <summary>
        /// 异步发送数据。
        /// <para>在<see cref="ITcpClient"/>时，如果使用独立线程发送，则不会触发异常。</para>
        /// <para>在<see cref="ITcpClientBase"/>时，相当于<see cref="Socket.BeginSend(byte[], int, int, SocketFlags, out SocketError, System.AsyncCallback, object)"/>。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(byte[] buffer, int offset, int length);

        /// <summary>
        /// 异步发送数据。
        /// <para>在<see cref="ITcpClient"/>时，如果使用独立线程发送，则不会触发异常。</para>
        /// <para>在<see cref="ITcpClientBase"/>时，相当于<see cref="Socket.BeginSend(byte[], int, int, SocketFlags, out SocketError, System.AsyncCallback, object)"/>。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(byte[] buffer);

        /// <summary>
        /// 异步发送数据。
        /// <para>在<see cref="ITcpClient"/>时，如果使用独立线程发送，则不会触发异常。</para>
        /// <para>在<see cref="ITcpClientBase"/>时，相当于<see cref="Socket.BeginSend(byte[], int, int, SocketFlags, out SocketError, System.AsyncCallback, object)"/>。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="byteBlock">数据块</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(ByteBlock byteBlock);
    }

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


    /// <summary>
    /// 发送等待接口
    /// </summary>
    public interface IWaitSender : ISendBase
    {
        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        byte[] SendThenReturn(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        byte[] SendThenReturn(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        byte[] SendThenReturn(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<byte[]> SendThenReturnAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default);
    }
}