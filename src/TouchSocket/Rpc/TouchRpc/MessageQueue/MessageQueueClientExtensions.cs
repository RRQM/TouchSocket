using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// MessageQueueClientExtensions
    /// </summary>
    public static class MessageQueueClientExtensions
    {
        /// <summary>
        /// 获取或设置MessageQueueClient的注入键。
        /// </summary>
        public static readonly DependencyProperty<MessageQueueClient> MessageQueueClientProperty =
            DependencyProperty<MessageQueueClient>.Register("MessageQueueClient", typeof(MessageQueueClientExtensions), null);

        /// <summary>
        /// 获取MessageQueueClient
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static MessageQueueClient GetMessageQueueClient<TClient>(this TClient client) where TClient : IDependencyTouchRpc, IDependencyObject
        {
            return client.GetValue(MessageQueueClientProperty);
        }
    }
}