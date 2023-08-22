using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 监听配置
    /// </summary>
    public class ListenOption
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 监听地址
        /// </summary>
        public IPHost IpHost { get; set; }

        /// <summary>
        /// 接收、发送缓存大小。同时也是内存池借鉴尺寸。
        /// </summary>
        public int BufferLength { get; set; } = 1024 * 64;

        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// 接收类型
        /// </summary>
        public ReceiveType ReceiveType { get; set; } = ReceiveType.Iocp;

        /// <summary>
        /// 是否使用地址复用
        /// </summary>
        public bool ReuseAddress { get; set; }

        /// <summary>
        /// Tcp处理并发连接时最大半连接队列
        /// </summary>
        public int Backlog { get; set; } = 100;

        /// <summary>
        /// 禁用延迟发送
        /// </summary>
        public bool? NoDelay { get; set; }

        /// <summary>
        /// 是否使用ssl加密
        /// </summary>
        public bool UseSsl => this.ServiceSslOption != null;

        /// <summary>
        /// 用于Ssl加密的证书
        /// </summary>
        public ServiceSslOption ServiceSslOption { get; set; }

        /// <summary>
        /// 配置Tcp适配器
        /// </summary>
        public Func<SingleStreamDataHandlingAdapter> TcpAdapter { get; set; } =
            () => new NormalDataHandlingAdapter();
    }
}
