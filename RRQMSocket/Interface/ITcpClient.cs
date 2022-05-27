using RRQMCore;
using System.Threading.Tasks;

namespace RRQMSocket
{
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
        /// 远程IPHost。
        /// </summary>
        IPHost RemoteIPHost { get; }

        /// <summary>
        /// 客户端配置
        /// </summary>
        RRQMConfig Config { get; }

        /// <summary>
        /// 独立线程发送
        /// </summary>
        bool SeparateThreadSend { get; }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        ITcpClient Connect(int timeout = 5000);

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        Task<ITcpClient> ConnectAsync(int timeout = 5000);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        ITcpClient Setup(RRQMConfig clientConfig);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="ipHost"></param>
        /// <returns></returns>
        ITcpClient Setup(string ipHost);
    }
}
