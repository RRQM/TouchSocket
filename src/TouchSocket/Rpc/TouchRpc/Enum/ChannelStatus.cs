namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 通道状态
    /// </summary>
    public enum ChannelStatus : byte
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default = 0,

        /// <summary>
        /// 继续下移
        /// </summary>
        Moving = 1,

        /// <summary>
        /// 超时
        /// </summary>
        Overtime = 2,

        /// <summary>
        /// 继续
        /// </summary>
        HoldOn = 3,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel = 4,

        /// <summary>
        /// 完成
        /// </summary>
        Completed = 5,

        /// <summary>
        /// 已释放
        /// </summary>
        Disposed = 6
    }
}
