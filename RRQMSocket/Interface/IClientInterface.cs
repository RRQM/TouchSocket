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
using RRQMCore.ByteManager;

using RRQMCore.Log;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 终端接口
    /// </summary>
    public interface IClient : IDisposable
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; }
    }

    /// <summary>
    /// 定制协议的终端接口
    /// </summary>
    public interface IProtocolClient : ITokenClient, IProtocolClientBase
    {
        /// <summary>
        /// 创建一个和其他客户端的通道
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public Channel CreateChannel(string clientID);

        /// <summary>
        /// 创建一个和其他客户端的通道
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Channel CreateChannel(string clientID, int id);
    }

    /// <summary>
    /// 协议客户端基类
    /// </summary>
    public interface IProtocolClientBase : ITokenClientBase
    {
        /// <summary>
        /// 添加协议订阅
        /// </summary>
        /// <param name="subscriber"></param>
        void AddProtocolSubscriber(SubscriberBase subscriber);

        /// <summary>
        /// 判断使用该ID的Channel是否存在。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ChannelExisted(int id);

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <returns></returns>
        Channel CreateChannel();
        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="id">指定ID</param>
        /// <returns></returns>
        Channel CreateChannel(int id);

        /// <summary>
        /// 移除协议订阅
        /// </summary>
        /// <param name="subscriber"></param>
        void RemoveProtocolSubscriber(SubscriberBase subscriber);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        void Send(short protocol, byte[] buffer);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void Send(short protocol, byte[] buffer, int offset, int length);

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataByteBlock"></param>
        void Send(short protocol, ByteBlock dataByteBlock);

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="protocol"></param>
        void Send(short protocol);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        void SendAsync(short protocol, byte[] buffer);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void SendAsync(short protocol, byte[] buffer, int offset, int length);

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataByteBlock"></param>
        void SendAsync(short protocol, ByteBlock dataByteBlock);

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="protocol"></param>
        void SendAsync(short protocol);

        /// <summary>
        /// 发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Result SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata = default);

        /// <summary>
        /// 异步发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<Result> SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata = default);

        /// <summary>
        /// 订阅通道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool TrySubscribeChannel(int id, out Channel channel);
    }

    /// <summary>
    /// 服务器辅助类接口
    /// </summary>
    public interface ISocketClient : ITcpClientBase, IClientSender, IIDSender
    {
        /// <summary>
        /// 用于索引的ID
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 包含此辅助类的主服务器类
        /// </summary>
        TcpServiceBase Service { get; }
    }

    /// <summary>
    /// TCP客户端终端接口
    /// </summary>
    public interface ITcpClient : ITcpClientBase, IClientSender
    {
        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        event RRQMMessageEventHandler<ITcpClient> Connected;

        /// <summary>
        /// 准备连接的时候
        /// </summary>
        event RRQMTcpClientConnectingEventHandler<ITcpClient> Connecting;

        /// <summary>
        /// 断开连接
        /// </summary>
        event RRQMMessageEventHandler<ITcpClient> Disconnected;

        /// <summary>
        /// 客户端配置
        /// </summary>
        TcpClientConfig ClientConfig { get; }

        /// <summary>
        /// 独立线程发送
        /// </summary>
        bool SeparateThreadSend { get; }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        ITcpClient Connect();

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        Task<ITcpClient> ConnectAsync();

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        ITcpClient Disconnect();

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        ITcpClient Setup(TcpClientConfig clientConfig);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="ipHost"></param>
        /// <returns></returns>
        ITcpClient Setup(string ipHost);
    }

    /// <summary>
    /// TCP客户端接口
    /// </summary>
    public interface ITcpClientBase : IClient
    {
        /// <summary>
        /// 缓存池大小
        /// </summary>
        int BufferLength { get; }

        /// <summary>
        /// 是否允许自由调用<see cref="SetDataHandlingAdapter"/>进行赋值。
        /// </summary>
        bool CanSetDataHandlingAdapter { get; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        DataHandlingAdapter DataHandlingAdapter { get; }

        /// <summary>
        /// IP地址
        /// </summary>
        string IP { get; }

        /// <summary>
        /// 主通信器
        /// </summary>
        Socket MainSocket { get; }

        /// <summary>
        /// IP及端口号
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 判断是否在线
        /// </summary>
        bool Online { get; }

        /// <summary>
        /// 端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 接收模式
        /// </summary>
        public ReceiveType ReceiveType { get; }

        /// <summary>
        /// 使用Ssl加密
        /// </summary>
        bool UseSsl { get; }
        /// <summary>
        /// 关闭Socket信道，并随后释放资源
        /// </summary>
        void Close();

        /// <summary>
        /// 获取流，在正常模式下为<see cref="System.Net.Sockets.NetworkStream"/>，在Ssl模式下为<see cref="SslStream"/>。
        /// </summary>
        /// <returns></returns>
        Stream GetStream();

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        void SetDataHandlingAdapter(DataHandlingAdapter adapter);

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        void Shutdown(SocketShutdown how);
    }

    /// <summary>
    /// 具有验证功能的终端接口
    /// </summary>
    public interface ITokenClient : ITcpClient, ITokenClientBase
    {
        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        ITcpClient Connect(string verifyToken, CancellationToken token = default);
    }

    /// <summary>
    /// Token客户端基类
    /// </summary>
    public interface ITokenClientBase : ITcpClientBase
    {
    }
}
