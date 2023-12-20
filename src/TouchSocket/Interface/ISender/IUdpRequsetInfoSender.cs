using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TouchSocket.Core;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IUdpRequsetInfoSender
    /// </summary>
    public interface IUdpRequsetInfoSender
    {
        /// <summary>
        /// 同步发送数据。
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="requestInfo">解析对象</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Send(EndPoint endPoint,IRequestInfo requestInfo);

        /// <summary>
        /// 异步发送数据。
        /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="requestInfo">解析对象</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        Task SendAsync(EndPoint endPoint,IRequestInfo requestInfo);
    }
}
