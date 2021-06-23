//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC调用设置
    /// </summary>
    public class InvokeOption
    {
        static InvokeOption()
        {
            onlySend = new InvokeOption();
            onlySend.WaitTime = 5;
            onlySend.FeedbackType = FeedbackType.OnlySend;

            waitSend = new InvokeOption();
            waitSend.WaitTime = 5;
            waitSend.FeedbackType = FeedbackType.WaitSend;

            waitInvoke = new InvokeOption();
            waitInvoke.WaitTime = 5;
            waitInvoke.FeedbackType = FeedbackType.WaitInvoke;
        }

        private static InvokeOption onlySend;

        /// <summary>
        /// 默认设置。
        /// WaitTime=5
        /// </summary>
        public static InvokeOption OnlySend { get { return onlySend; } }

        private static InvokeOption waitSend;

        /// <summary>
        /// 默认设置。
        /// WaitTime=5
        /// </summary>
        public static InvokeOption WaitSend { get { return waitSend; } }

        private static InvokeOption waitInvoke;

        /// <summary>
        /// 默认设置。
        /// WaitTime=5
        /// </summary>
        public static InvokeOption WaitInvoke { get { return waitInvoke; } }

        /// <summary>
        /// 调用等待时长
        /// </summary>
        public int WaitTime { get; set; }

        /// <summary>
        /// 调用反馈
        /// </summary>
        public FeedbackType FeedbackType { get; set; }
    }
}