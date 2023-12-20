using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 周期包适配
    /// </summary>
    public class PeriodPackageAdapter : SingleStreamDataHandlingAdapter
    {
        private readonly ConcurrentQueue<byte[]> m_bytes = new ConcurrentQueue<byte[]>();
        private long m_count;

        /// <inheritdoc/>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            this.m_bytes.Enqueue(byteBlock.ToArray());
            Interlocked.Increment(ref this.m_count);
            Task.Run(this.DelayGo);
        }

        private async Task DelayGo()
        {
            await Task.Delay(this.CacheTimeout);
            if (Interlocked.Decrement(ref this.m_count) == 0)
            {
                using (var byteBlock = new ByteBlock())
                {
                    while (this.m_bytes.TryDequeue(out var bytes))
                    {
                        byteBlock.Write(bytes);
                    }

                    byteBlock.SeekToStart();

                    try
                    {
                        this.GoReceived(byteBlock, default);
                    }
                    catch (Exception ex)
                    {
                        this.OnError(ex, ex.Message, true, true);
                    }
                }
            }
        }
    }
}