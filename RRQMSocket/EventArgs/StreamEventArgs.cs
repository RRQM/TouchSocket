using RRQMCore;
using System.IO;

namespace RRQMSocket
{
    /// <summary>
    /// 流事件参数
    /// </summary>
    public class StreamEventArgs : MesEventArgs
    {
        private Metadata metadata;

        private StreamInfo streamInfo;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="streamInfo"></param>
        public StreamEventArgs(Metadata metadata, StreamInfo streamInfo)
        {
            this.metadata = metadata;
            this.streamInfo = streamInfo;
        }

        /// <summary>
        /// 用于接收流的容器
        /// </summary>
        public Stream Bucket { get; set; }

        /// <summary>
        /// 用于可传输的元数据
        /// </summary>
        public Metadata Metadata
        {
            get { return metadata; }
        }
        /// <summary>
        /// 流信息
        /// </summary>
        public StreamInfo StreamInfo
        {
            get { return streamInfo; }
        }
    }
}