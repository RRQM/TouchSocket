using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 创建通道事件类
    /// </summary>
    public class CreateChannelEventArgs : PluginEventArgs
    {
        /// <summary>
        /// 创建通道事件类
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="metadata"></param>
        public CreateChannelEventArgs(int channelId, Metadata metadata)
        {
            this.ChannelId = channelId;
            this.Metadata = metadata;
        }

        /// <summary>
        /// 通道Id
        /// </summary>
        public int ChannelId { get; private set; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata { get; private set; }
    }
}