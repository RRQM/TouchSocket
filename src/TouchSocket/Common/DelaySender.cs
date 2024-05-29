//using System;
//using System.Collections.Generic;
//using System.Linq;
//using TouchSocket.Core;
//using System.Threading.Tasks;
//using System.Collections.Concurrent;

//namespace TouchSocket.Sockets
//{
//    /// <summary>
//    /// 延迟发送器
//    /// </summary>
//    internal sealed class DelaySender : DisposableObject
//    {
//        private readonly ConcurrentQueue<ArraySegment<byte>> m_queueDatas = new ConcurrentQueue<ArraySegment<byte>>();
//        private readonly Action<byte[], int, int> m_action;
//        private readonly AsyncAutoResetEvent m_resetEvent = new AsyncAutoResetEvent(false);

//        public DelaySender(Action<byte[], int, int> action)
//        {
//            Task.Run(this.BeginSend);
//            this.m_action = action ?? throw new ArgumentNullException(nameof(action));
//        }

//        /// <summary>
//        /// 队列长度
//        /// </summary>
//        public int QueueCount => this.m_queueDatas.Count;

//        /// <summary>
//        /// 发送
//        /// </summary>
//        public void Send(ArraySegment<byte> bytes)
//        {
//            this.m_queueDatas.Enqueue(bytes);
//            this.m_resetEvent.Set();
//        }

//        private async Task BeginSend()
//        {
//            while (!this.DisposedValue)
//            {
//                var buffer = BytePool.Default.Rent(this.DelayLength);
//                try
//                {
//                    if (this.TryGet(buffer, out var asyncByte))
//                    {
//                        this.m_action.Invoke(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
//                    }
//                    else
//                    {
//                        await this.m_resetEvent.WaitOneAsync().ConfigureFalseAwait();
//                    }
//                }
//                catch
//                {
//                }
//                finally
//                {
//                    BytePool.Default.Return(buffer);
//                }
//            }
//        }

//        private bool TryGet(byte[] buffer, out QueueDataBytes asyncByteDe)
//        {
//            var len = 0;
//            var surLen = buffer.Length;
//            while (true)
//            {
//                if (this.m_queueDatas.TryPeek(out var asyncB))
//                {
//                    if (surLen > asyncB.Length)
//                    {
//                        if (this.m_queueDatas.TryDequeue(out var asyncByte))
//                        {
//                            Array.Copy(asyncByte.Buffer, asyncByte.Offset, buffer, len, asyncByte.Length);
//                            len += asyncByte.Length;
//                            surLen -= asyncByte.Length;
//                        }
//                    }
//                    else if (asyncB.Length > buffer.Length)
//                    {
//                        if (len > 0)
//                        {
//                            break;
//                        }
//                        else
//                        {
//                            asyncByteDe = asyncB;
//                            return true;
//                        }
//                    }
//                    else
//                    {
//                        break;
//                    }
//                }
//                else
//                {
//                    if (len > 0)
//                    {
//                        break;
//                    }
//                    else
//                    {
//                        asyncByteDe = default;
//                        return false;
//                    }
//                }
//            }
//            asyncByteDe = new QueueDataBytes(buffer, 0, len);
//            return true;
//        }
//    }
//}
