using RRQMCore;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 接收流数据
    /// </summary>
    public class StreamOperationEventArgs : StreamEventArgs
    {
        private bool isPermitOperation = true;

        private StreamOperator streamOperator;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <param name="streamInfo"></param>
        public StreamOperationEventArgs(StreamOperator streamOperator, Metadata metadata, StreamInfo streamInfo) : base(metadata, streamInfo)
        {
            this.streamOperator = streamOperator ?? throw new ArgumentNullException(nameof(streamOperator));
        }

        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool IsPermitOperation
        {
            get { return isPermitOperation; }
            set { isPermitOperation = value; }
        }

        /// <summary>
        /// 流操作
        /// </summary>
        public StreamOperator StreamOperator
        {
            get { return streamOperator; }
        }
    }
}