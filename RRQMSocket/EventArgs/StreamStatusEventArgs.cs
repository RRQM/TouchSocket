using RRQMCore;

namespace RRQMSocket
{
    /// <summary>
    /// 具有返回状态的流
    /// </summary>
    public class StreamStatusEventArgs : StreamEventArgs
    {

        private ChannelStatus status;
        /// <summary>
        /// 状态
        /// </summary>
        public ChannelStatus Status
        {
            get { return status; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status"></param>
        /// <param name="metadata"></param>
        /// <param name="streamInfo"></param>
        public StreamStatusEventArgs(ChannelStatus status, Metadata metadata, StreamInfo streamInfo) :base(metadata,streamInfo)
        {
            this.status = status;
        }
    }
}
