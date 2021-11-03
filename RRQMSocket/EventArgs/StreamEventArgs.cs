using RRQMCore;
using System.IO;

namespace RRQMSocket
{
    /// <summary>
    /// 流事件参数
    /// </summary>
    public class StreamEventArgs : MesEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public StreamEventArgs()
        {
            this.metadata = new Metadata();
        }

        private Metadata metadata;
        /// <summary>
        /// 用于可传输的元数据
        /// </summary>
        public Metadata Metadata
        {
            get { return metadata; }
            internal set { metadata = value; }
        }

        /// <summary>
        /// 流信息
        /// </summary>
        public StreamInfo StreamInfo { get; internal set; }

        /// <summary>
        /// 用于接收流的容器
        /// </summary>
        public Stream Bucket { get; set; }
    }
}
