namespace RRQMSocket
{
    /// <summary>
    /// 接收流数据
    /// </summary>
    public class StreamOperationEventArgs : StreamEventArgs
    {
        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool IsPermitOperation { get; set; }

        /// <summary>
        /// 流操作
        /// </summary>
        public StreamOperator StreamOperator { get; internal set; }
    }
}
