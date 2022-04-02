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
using RRQMCore.Dependency;
using RRQMCore.Log;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 插件接口
    /// </summary>
    public interface IPlugin:IDisposable
    {
        /// <summary>
        /// 插件执行顺序
        /// <para>该属性值越小，越靠前执行。值相等时，按添加先后顺序</para>
        /// <para>该属性效果，仅在<see cref="IPluginsManager.Add(IPlugin)"/>之前设置有效。</para>
        /// </summary>
        public byte Order { get; set; }

        /// <summary>
        /// 日志记录器。
        /// <para>在<see cref="IPluginsManager.Add(IPlugin)"/>之前如果没有赋值的话，随后会用<see cref="IContainer.Resolve{T}"/>填值</para>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// 包含此插件的插件管理器
        /// </summary>
        IPluginsManager PluginsManager { get; set; }
    }

    /// <summary>
    /// Udp会话插件
    /// </summary>
    public interface IUdpSessionPlugin : IPlugin
    {
        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e);
    }

    /// <summary>
    /// Tcp系插件接口
    /// </summary>
    public interface ITcpPlugin : IPlugin
    {
        /// <summary>
        /// 客户端连接成功后触发    
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnConnected(ITcpClientBase client, RRQMEventArgs e);

        /// <summary>
        ///在即将完成连接时触发。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e);

        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e);

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e);

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnSendingData(ITcpClientBase client, SendingEventArgs e);

        /// <summary>
        /// 当Client的ID被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnIDChanged(ITcpClientBase client, RRQMEventArgs e);
    }

    /// <summary>
    /// Token系插件接口
    /// </summary>
    public interface ITokenPlugin : IPlugin
    {
        /// <summary>
        /// 在收到Token数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnHandleTokenData(ITcpClientBase client, ReceivedDataEventArgs e);

        /// <summary>
        /// 在完成握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnHandshaked(ITcpClientBase client, MesEventArgs e);

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnVerifyToken(ITcpClientBase client, VerifyOptionEventArgs e);

        /// <summary>
        /// 收到非正常连接。
        /// 一般地，这是由其他类型客户端发起的连接。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnAbnormalVerify(ITcpClientBase client, ReceivedDataEventArgs e);
    }

    /// <summary>
    /// Protocol系插件接口
    /// </summary>
    public interface IProtocolPlugin : IPlugin
    {
        /// <summary>
        /// 收到协议数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnHandleProtocolData(ITcpClientBase client, ProtocolDataEventArgs e);

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnStreamTransfering(ITcpClientBase client, StreamOperationEventArgs e);

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnStreamTransfered(ITcpClientBase client, StreamStatusEventArgs e);
    }
}
