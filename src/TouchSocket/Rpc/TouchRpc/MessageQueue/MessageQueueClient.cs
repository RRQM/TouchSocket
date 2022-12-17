using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// MessageQueueClient
    /// </summary>
    public abstract class MessageQueueClient
    {
        /// <summary>
        /// 序列化转换器。
        /// </summary>
        public BytesConverter Converter { get; set; }

        ///// <summary>
        ///// 队列
        ///// </summary>
        ///// <param name="queue"></param>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        //public abstract void Publish(string queue, byte[] buffer,int offset,int length);

        //public abstract void Subscribe<T>(string queue,Action<> data);
    }

    internal class InternalMessageQueueClient : MessageQueueClient
    {
        private RpcActor m_rpcActor;
        public InternalMessageQueueClient(RpcActor rpcActor, BytesConverter converter)
        {
            m_rpcActor = rpcActor;
            Converter = converter;

        }
    }
}
