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
using System.Net.Sockets;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IRequsetInfoSend
    /// </summary>
    public interface IRequsetInfoSender
    {
        /// <summary>
        /// 同步发送数据。
        /// <para>内部已经封装Ssl和发送长度检测，即：调用完成即表示数据全部发送完毕。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="requestInfo">解析对象</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(IRequestInfo requestInfo);

        /// <summary>
        /// 异步发送数据。
        /// <para>在<see cref="ITcpClient"/>时，如果使用独立线程发送，则不会触发异常。</para>
        /// <para>在<see cref="ITcpClientBase"/>时，相当于<see cref="Socket.BeginSend(byte[], int, int, SocketFlags, out SocketError, System.AsyncCallback, object)"/>。</para>
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="requestInfo">解析对象</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void SendAsync(IRequestInfo requestInfo);
    }
}
