using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 线程安全的释放模型。无论是<see cref="IDisposable"/>还是<see cref="GC"/>执行，都只会触发1次<see cref="SafetyDispose(bool)"/>方法。
    /// </summary>
    public abstract class SafetyDisposableObject : DisposableObject
    {
        private int m_count = 0;

        /// <inheritdoc/>
        protected override sealed void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }

            base.Dispose(disposing);

            if (Interlocked.Increment(ref this.m_count) == 1)
            {
                this.SafetyDispose(disposing);
            }
        }

        /// <summary>
        /// 线程安全模式的释放，无论是<see cref="IDisposable"/>还是<see cref="GC"/>执行，都只会触发一次
        /// </summary>
        protected abstract void SafetyDispose(bool disposing);
    }
}