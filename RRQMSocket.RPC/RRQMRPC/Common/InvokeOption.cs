//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using RRQMCore.Serialization;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC调用设置
    /// </summary>
    public class InvokeOption
    {
        private static InvokeOption onlySend;

        private static InvokeOption waitInvoke;

        private static InvokeOption waitSend;

        private int timeout = 5000;

        static InvokeOption()
        {
            onlySend = new InvokeOption();
            onlySend.FeedbackType = FeedbackType.OnlySend;

            waitSend = new InvokeOption();
            waitSend.FeedbackType = FeedbackType.WaitSend;

            waitInvoke = new InvokeOption();
            waitInvoke.FeedbackType = FeedbackType.WaitInvoke;
        }
        /// <summary>
        /// 默认设置。
        /// Timeout=5000 ms
        /// </summary>
        public static InvokeOption OnlySend { get { return onlySend; } }
        /// <summary>
        /// 默认设置。
        /// Timeout=5000 ms
        /// </summary>
        public static InvokeOption WaitInvoke { get { return waitInvoke; } }

        /// <summary>
        /// 默认设置。
        /// Timeout=5000 ms
        /// </summary>
        public static InvokeOption WaitSend { get { return waitSend; } }
        /// <summary>
        /// 调用反馈
        /// </summary>
        public FeedbackType FeedbackType { get; set; }

        private SerializationType serializationType = SerializationType.RRQMBinary;

        /// <summary>
        /// RRQMRPC序列化类型
        /// </summary>
        public SerializationType SerializationType
        {
            get { return serializationType; }
            set { serializationType = value; }
        }

        private InvokeType invokeType= InvokeType.GlobalInstance;
        /// <summary>
        /// 调用类型
        /// </summary>
        public InvokeType InvokeType
        {
            get { return invokeType; }
            set { invokeType = value; }
        }



        /// <summary>
        /// 调用超时，
        /// min=1000，默认5000 ms
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set
            {
                if (value < 1000)
                {
                    value = 1000;
                }
                timeout = value;
            }
        }
    }
}