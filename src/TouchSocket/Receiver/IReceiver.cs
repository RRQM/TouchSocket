using System;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IReceiver
    /// </summary>
    public interface IReceiver : IDisposable
    {
        /// <summary>
        /// 异步等待并读取
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ReceiverResult> ReadAsync(CancellationToken token);

#if NET6_0_OR_GREATER
        /// <summary>
        /// 值异步等待并读取
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ValueTask<ReceiverResult> ValueReadAsync(CancellationToken token);
#endif
    }
}