using TouchSocket.Core;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// SmtpMessageEventArgs
    /// </summary>
    public class SmtpMessageEventArgs : PluginEventArgs
    {
        /// <summary>
        /// SmtpMessageEventArgs
        /// </summary>
        /// <param name="message"></param>
        public SmtpMessageEventArgs(SmtpMessage message)
        {
            this.SmtpMessage = message;
        }

        public SmtpMessage SmtpMessage { get; }
    }
}
