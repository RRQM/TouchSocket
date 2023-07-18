namespace TouchSocket.Core
{
    /// <summary>
    /// 异步等待的AutoResetEvent
    /// WaitOneAsync方法会返回一个task，通过await方式等待
    /// </summary>
    public class AsyncAutoResetEvent : AsyncResetEvent
    {
        /// <summary>
        /// 异步等待的AutoResetEvent
        /// WaitOneAsync方法会返回一个task，通过await方式等待
        /// </summary>
        public AsyncAutoResetEvent() : this(false) { }

        /// <summary>
        /// 异步等待的AutoResetEvent
        /// WaitOneAsync方法会返回一个task，通过await方式等待
        /// </summary>
        /// <param name="set"></param>
        public AsyncAutoResetEvent(bool set) : base(set, true) { }
    }
}