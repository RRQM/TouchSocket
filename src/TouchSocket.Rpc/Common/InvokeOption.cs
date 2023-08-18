using System.Threading;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// Rpc调用设置
    /// </summary>
    public class InvokeOption : IInvokeOption
    {
        static InvokeOption()
        {
            OnlySend = new InvokeOption(timeout: 5000)
            {
                FeedbackType = FeedbackType.OnlySend
            };

            WaitSend = new InvokeOption(timeout: 5000)
            {
                FeedbackType = FeedbackType.WaitSend
            };

            WaitInvoke = new InvokeOption(timeout: 5000)
            {
                FeedbackType = FeedbackType.WaitInvoke
            };
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public InvokeOption()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="timeout"></param>
        public InvokeOption(int timeout)
        {
            this.Timeout = timeout;
        }

        /// <summary>
        /// 默认设置。
        /// Timeout=5000ms
        /// </summary>
        public static InvokeOption OnlySend { get; }

        /// <summary>
        /// 默认设置。
        /// Timeout=5000ms
        /// </summary>
        public static InvokeOption WaitInvoke { get; }

        /// <summary>
        /// 默认设置。
        /// Timeout=5000 ms
        /// </summary>
        public static InvokeOption WaitSend { get; }

        /// <summary>
        /// 调用反馈
        /// </summary>
        public FeedbackType FeedbackType { get; set; } = FeedbackType.WaitInvoke;

        /// <summary>
        /// 调用超时，
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// 可以取消的调用令箭
        /// </summary>
        public CancellationToken Token { get; set; } = CancellationToken.None;
    }
}
