using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 触发器队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TriggerQueue<T> : DisposableObject
    {
        private readonly ReaderWriterLockSlim m_lockSlim;
        private readonly ConcurrentQueue<T> m_queue;
        private readonly Timer m_timer;
        private volatile bool m_sending;

        /// <summary>
        ///  触发器队列
        /// </summary>
        public TriggerQueue()
        {
            m_lockSlim = new ReaderWriterLockSlim();
            m_queue = new ConcurrentQueue<T>();
            m_timer = new Timer(TimerRun, null, 10, 10);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TriggerQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// 出队列处理。
        /// </summary>
        public Action<T> OnDequeue { get; set; }

        /// <summary>
        /// 发生错误
        /// </summary>
        public Action<Exception> OnError { get; set; }

        /// <summary>
        /// 是否处于发送状态
        /// </summary>
        public bool Sending
        {
            get
            {
                using (new ReadLock(m_lockSlim))
                {
                    return m_sending;
                }
            }

            private set
            {
                using (new WriteLock(m_lockSlim))
                {
                    m_sending = value;
                }
            }
        }

        /// <summary>
        /// 发送
        /// </summary>
        public void Enqueue(T data)
        {
            m_queue.Enqueue(data);
            if (SwitchToRun())
            {
                ThreadPool.QueueUserWorkItem(BeginTrigger);
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            m_timer.SafeDispose();
            m_queue.Clear();
            base.Dispose(disposing);
        }

        private void BeginTrigger(object o)
        {
            while (true)
            {
                try
                {
                    if (m_queue.TryDequeue(out T data))
                    {
                        OnDequeue?.Invoke(data);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
            }
            Sending = false;
        }

        private bool SwitchToRun()
        {
            using (new ReadLock(m_lockSlim))
            {
                if (m_sending)
                {
                    return false;
                }
                else
                {
                    m_sending = true;
                    return true;
                }
            }
        }

        private void TimerRun(object state)
        {
            if (SwitchToRun())
            {
                BeginTrigger(null);
            }
        }
    }
}