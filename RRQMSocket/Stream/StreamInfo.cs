namespace RRQMSocket
{
    /// <summary>
    /// 流信息
    /// </summary>
    public struct StreamInfo
    {
        private long size;

        private string streamType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="size"></param>
        /// <param name="streamType"></param>
        public StreamInfo(long size, string streamType)
        {
            this.size = size;
            this.streamType = streamType;
        }

        /// <summary>
        /// 流长度
        /// </summary>
        public long Size
        {
            get { return size; }
        }

        /// <summary>
        /// 流类型
        /// </summary>
        public string StreamType
        {
            get { return streamType; }
        }
    }
}