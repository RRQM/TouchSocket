namespace TouchSocket.Core
{
    /// <summary>
    /// 一个手动恢复的异步通知事件
    /// </summary>
    public class AsyncManualResetEvent : AsyncResetEvent
    {
        /// <summary>
        /// 一个手动恢复的异步通知事件
        /// </summary>
        public AsyncManualResetEvent() : this(false) { }

        /// <summary>
        ///  一个手动恢复的异步通知事件
        /// </summary>
        /// <param name="set"></param>
        public AsyncManualResetEvent(bool set) : base(set, false) { }
    }
}