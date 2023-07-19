namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IActor
    /// </summary>
    public interface IActor
    {
        /// <summary>
        /// 包含当前Actor的父容器。
        /// </summary>
        public IDmtpActor DmtpActor { get; }

        /// <summary>
        /// 处理收到的消息。
        /// </summary>
        /// <param name="message"></param>
        /// <returns>当满足本协议时，应当返回<see langword="true"/>，其他时候应该返回<see langword="false"/>.</returns>
        public bool InputReceivedData(DmtpMessage message);
    }
}
