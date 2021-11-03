namespace RRQMSocket
{
    /// <summary>
    /// 具有返回状态的流
    /// </summary>
    public class StreamStatusEventArgs : StreamEventArgs
    {
        /// <summary>
        /// 状态
        /// </summary>
        public ChannelStatus Status { get; internal set; }
    }
}
