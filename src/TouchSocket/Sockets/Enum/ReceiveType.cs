namespace TouchSocket.Sockets
{
    /// <summary>
    /// 接收类型
    /// </summary>
    public enum ReceiveType : byte
    {
        /// <summary>
        /// 该模式下会自动接收数据，然后主动触发。
        /// </summary>
        Auto,

        /// <summary>
        /// 在该模式下，不会投递接收申请，用户可通过<see cref="ITcpClientBase.GetStream"/>，获取到流以后，自己处理接收。
        /// <para>注意：连接端不会感知主动断开</para>
        /// </summary>
        None
    }
}
