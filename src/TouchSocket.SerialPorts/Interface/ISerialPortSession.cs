using TouchSocket.Sockets;
using TouchSocket.Core;

namespace TouchSocket.SerialPorts
{
    /// <summary>
    /// 定义了一个串行端口会话接口，继承自多个与客户端、插件、配置、在线状态、连接状态和关闭操作相关的接口。
    /// </summary>
    public interface ISerialPortSession : IClient, IPluginObject, ISetupConfigObject, IOnlineClient, IConnectableClient, IClosableClient
    {
    }
}
