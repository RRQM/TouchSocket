namespace TouchSocket.Core
{
    /// <summary>
    /// 消息事件
    /// </summary>
    public class MsgPermitEventArgs : PluginEventArgs
    {
        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="mes"></param>
        public MsgPermitEventArgs(string mes)
        {
            this.Message = mes;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MsgPermitEventArgs()
        {
        }

        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool IsPermitOperation { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}