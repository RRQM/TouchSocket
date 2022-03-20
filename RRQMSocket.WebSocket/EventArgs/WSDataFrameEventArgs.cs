using RRQMCore;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// WS数据事件类
    /// </summary>
    public class WSDataFrameEventArgs : RRQMEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataFrame"></param>
        public WSDataFrameEventArgs(WSDataFrame dataFrame)
        {
            this.DataFrame = dataFrame;
        }

        /// <summary>
        /// WS数据帧。
        /// </summary>
        public WSDataFrame DataFrame { get; }
    }
}
