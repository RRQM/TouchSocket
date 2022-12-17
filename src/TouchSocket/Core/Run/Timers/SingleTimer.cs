using System;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 不可重入的Timer
    /// </summary>
    public class SingleTimer : DisposableObject
    {
        private readonly Action<SingleTimer> m_action1;
        private readonly Action<SingleTimer, object> m_action2;
        private readonly object m_state;
        private readonly Timer m_timer;
        private readonly Action m_action3;
        private int m_signal = 1;

        /// <summary>
        /// 是否暂停执行。
        /// </summary>
        public bool Pause { get; set; }

        /// <summary>
        /// 自启动以来执行的次数。
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(int period, Action action)
        {
            m_timer = new Timer(OnTimer, null, 0, period);
            m_action3 = action;
            m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(TimeSpan period, Action action)
        {
            m_timer = new Timer(OnTimer, null, TimeSpan.Zero, period);
            m_action3 = action;
            m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(int period, Action<SingleTimer> action)
        {
            m_timer = new Timer(OnTimer, null, 0, period);
            m_action1 = action;
            m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(int period, Action<SingleTimer, object> action)
        {
            m_timer = new Timer(OnTimer, null, 0, period);
            m_action2 = action;
            m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, TimeSpan period, Action<SingleTimer> action)
        {
            m_timer = new Timer(OnTimer, state, TimeSpan.Zero, period);
            m_action1 = action;
            m_state = state;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, TimeSpan period, Action<SingleTimer, object> action)
        {
            m_timer = new Timer(OnTimer, state, TimeSpan.Zero, period);
            m_action2 = action;
            m_state = state;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, int period, Action<SingleTimer> action)
        {
            m_timer = new Timer(OnTimer, state, 0, period);
            m_action1 = action;
            m_state = state;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, int period, Action<SingleTimer, object> action)
        {
            m_timer = new Timer(OnTimer, state, 0, period);
            m_action2 = action;
            m_state = state;
        }

        private void OnTimer(object state)
        {
            if (Pause)
            {
                return;
            }
            if (Interlocked.Decrement(ref m_signal) == 0)
            {
                try
                {
                    Count++;
                    m_action1?.Invoke(this);
                    m_action2?.Invoke(this, m_state);
                    m_action3?.Invoke();
                }
                catch
                {
                }
                finally
                {
                    m_signal = 1;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            m_timer.SafeDispose();
            base.Dispose(disposing);
        }
    }
}