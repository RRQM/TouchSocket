using RRQMCore.ByteManager;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// TCP终端基础接口
    /// </summary>
    public interface ITcpClientBase : IClient, ISend, IDefaultSender
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        event RRQMTcpClientDisconnectedEventHandler<ITcpClientBase> Disconnected;

        /// <summary>
        /// 缓存池大小
        /// </summary>
        int BufferLength { get; }

        /// <summary>
        /// 适配器能接收的最大数据包长度。
        /// </summary>
        int MaxPackageSize { get; }

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
        /// 判断是否在线
        /// <para>该属性仅表示TCP状态是否在线</para>
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
        /// 中断终端，传递中断消息
        /// </summary>
        /// <param name="msg"></param>
        void Close(string msg);

        /// <summary>
        /// 获取流，在正常模式下为<see cref="NetworkStream"/>，在Ssl模式下为<see cref="SslStream"/>。
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
}
