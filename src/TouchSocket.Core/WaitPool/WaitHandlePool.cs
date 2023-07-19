//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 等待处理数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitHandlePool<T> : DisposableObject where T : IWaitResult
    {
        private readonly ConcurrentDictionary<long, WaitData<T>> m_waitDic;
        private readonly ConcurrentDictionary<long, WaitDataAsync<T>> m_waitDicAsync;
        private readonly ConcurrentQueue<WaitData<T>> m_waitQueue;
        private readonly ConcurrentQueue<WaitDataAsync<T>> m_waitQueueAsync;

        private long m_waitCount;

        private long m_waitReverseCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitHandlePool()
        {
            this.m_waitDic = new ConcurrentDictionary<long, WaitData<T>>();
            this.m_waitDicAsync = new ConcurrentDictionary<long, WaitDataAsync<T>>();
            this.m_waitQueue = new ConcurrentQueue<WaitData<T>>();
            this.m_waitQueueAsync = new ConcurrentQueue<WaitDataAsync<T>>();
        }

        /// <summary>
        /// 取消全部
        /// </summary>
        public void CancelAll()
        {
            foreach (var item in this.m_waitDic.Values)
            {
                item.Cancel();
            }
            foreach (var item in this.m_waitDicAsync.Values)
            {
                item.Cancel();
            }

        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="waitData"></param>
        public void Destroy(WaitData<T> waitData)
        {
            if (waitData.WaitResult==null)
            {
                return;
            }
            if (this.m_waitDic.TryRemove(waitData.WaitResult.Sign, out var wait))
            {
                if (wait.DisposedValue)
                {
                    return;
                }

                wait.Reset();
                this.m_waitQueue.Enqueue(wait);
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="waitData"></param>
        public void Destroy(WaitDataAsync<T> waitData)
        {
            if (waitData.WaitResult == null)
            {
                return;
            }
            if (this.m_waitDicAsync.TryRemove(waitData.WaitResult.Sign, out var wait))
            {
                if (wait.DisposedValue)
                {
                    return;
                }

                wait.Reset();
                this.m_waitQueueAsync.Enqueue(wait);
            }
        }

        /// <summary>
        ///  获取一个Sign为负数的可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitData<T> GetReverseWaitData(T result, bool autoSign = true)
        {
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = Interlocked.Decrement(ref this.m_waitReverseCount);
                }
                waitData.SetResult(result);
                this.m_waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            if (autoSign)
            {
                result.Sign = Interlocked.Decrement(ref this.m_waitReverseCount);
            }
            waitData.SetResult(result);
            this.m_waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个Sign为负数的可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitDataAsync<T> GetReverseWaitDataAsync(T result, bool autoSign = true)
        {
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = Interlocked.Decrement(ref this.m_waitReverseCount);
                }
                waitData.SetResult(result);
                this.m_waitDicAsync.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitDataAsync<T>();
            if (autoSign)
            {
                result.Sign = Interlocked.Decrement(ref this.m_waitReverseCount);
            }
            waitData.SetResult(result);
            this.m_waitDicAsync.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitData<T> GetWaitData(T result, bool autoSign = true)
        {
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = Interlocked.Increment(ref this.m_waitCount);
                }
                waitData.SetResult(result);
                this.m_waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            if (autoSign)
            {
                result.Sign = Interlocked.Increment(ref this.m_waitCount);
            }
            waitData.SetResult(result);
            this.m_waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitDataAsync<T> GetWaitDataAsync(T result, bool autoSign = true)
        {
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = Interlocked.Increment(ref this.m_waitCount);
                }
                waitData.SetResult(result);
                this.m_waitDicAsync.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitDataAsync<T>();
            if (autoSign)
            {
                result.Sign = Interlocked.Increment(ref this.m_waitCount);
            }
            waitData.SetResult(result);
            this.m_waitDicAsync.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        public bool SetRun(long sign)
        {
            var result = false;
            if (this.m_waitDic.TryGetValue(sign, out var waitData))
            {
                waitData.Set();
                result = true;
            }

            if (this.m_waitDicAsync.TryGetValue(sign, out var waitDataAsync))
            {
                waitDataAsync.Set();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="waitResult"></param>
        public bool SetRun(long sign, T waitResult)
        {
            var result = false;
            if (this.m_waitDic.TryGetValue(sign, out var waitData))
            {
                waitData.Set(waitResult);
                result = true;
            }

            if (this.m_waitDicAsync.TryGetValue(sign, out var waitDataAsync))
            {
                waitDataAsync.Set(waitResult);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="waitResult"></param>
        public bool SetRun(T waitResult)
        {
            var result = false;
            if (this.m_waitDic.TryGetValue(waitResult.Sign, out var waitData))
            {
                waitData.Set(waitResult);
                result = true;
            }

            if (this.m_waitDicAsync.TryGetValue(waitResult.Sign, out var waitDataAsync))
            {
                waitDataAsync.Set(waitResult);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            foreach (var item in this.m_waitDic.Values)
            {
                item.SafeDispose();
            }
            foreach (var item in this.m_waitQueue)
            {
                item.SafeDispose();
            }
            this.m_waitDic.Clear();

            this.m_waitQueue.Clear();
            base.Dispose(disposing);
        }
    }
}