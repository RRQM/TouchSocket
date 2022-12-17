using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// MessageQueuePlugin
    /// </summary>
    public class MessageQueuePlugin : TouchRpcPluginBase<IDependencyTouchRpc>
    {
        /// <summary>
        /// 定义元素的序列化和反序列化。
        /// <para>注意：Byte[]类型不用考虑。内部单独会做处理。</para>
        /// </summary>
        public BytesConverter Converter { get; private set; } = new BytesConverter();


        /// <summary>
        /// 定义元素的序列化和反序列化。
        /// <para>注意：Byte[]类型不用考虑。内部单独会做处理。</para>
        /// </summary>
        /// <param name="converter"></param>
        public void SetConverter(BytesConverter converter)
        {
            Converter = converter;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnHandshaked(IDependencyTouchRpc client, VerifyOptionEventArgs e)
        {
            client.SetValue(MessageQueueClientExtensions.MessageQueueClientProperty, new InternalMessageQueueClient(client.RpcActor, Converter));
            base.OnHandshaked(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnReceivedProtocolData(IDependencyTouchRpc client, ProtocolDataEventArgs e)
        {

        }
    }
}