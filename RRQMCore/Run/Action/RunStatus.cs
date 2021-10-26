namespace RRQMCore.Run
{
    /// <summary>
    /// 运行状态
    /// </summary>
    public enum RunStatus : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Running
        /// </summary>
        Running,

        /// <summary>
        /// Completed
        /// </summary>
        Completed,

        /// <summary>
        /// Pause
        /// </summary>
        Paused,

        /// <summary>
        /// Disposed
        /// </summary>
        Disposed
    }
}