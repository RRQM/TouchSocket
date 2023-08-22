using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpMessageEventArgs
    /// </summary>
    public class DmtpMessageEventArgs : PluginEventArgs
    {
        /// <summary>
        /// DmtpMessageEventArgs
        /// </summary>
        /// <param name="message"></param>
        public DmtpMessageEventArgs(DmtpMessage message)
        {
            this.DmtpMessage = message;
        }

        /// <summary>
        /// Dmtp消息
        /// </summary>
        public DmtpMessage DmtpMessage { get; }
    }
}
