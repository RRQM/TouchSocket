namespace RRQMSocket
{
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

        /// <summary>
        /// 插件管理器
        /// </summary>
        IPluginsManager PluginsManager { get; }
    }
}
