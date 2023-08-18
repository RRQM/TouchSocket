namespace TouchSocket.Dmtp
{
    /// <summary>
    /// Dmtp功能性组件接口
    /// </summary>
    public interface IDmtpFeature
    {
        /// <summary>
        /// 起始协议
        /// </summary>
        ushort StartProtocol { get; set; }

        /// <summary>
        /// 保留协议长度
        /// </summary>
        ushort ReserveProtocolSize { get; }
    }
}
