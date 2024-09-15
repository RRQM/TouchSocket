//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TouchSocket.Core
{

    /// <summary>
    /// WaitHandlePool 类用于管理具有等待句柄的资源，提供了一种线程安全的资源分配和回收机制。
    /// 它的目的是优化资源使用，通过重用资源来减少创建和销毁资源的开销。
    /// </summary>
    /// <typeparam name="T">资源类型，必须实现 IWaitHandle 接口。</typeparam>
    public class WaitHandlePool<T> : DisposableObject where T : IWaitHandle
    {
        private readonly ConcurrentDictionary<int, WaitData<T>> m_waitDic;
        private readonly ConcurrentDictionary<int, WaitDataAsync<T>> m_waitDicAsync;
        private readonly ConcurrentQueue<WaitData<T>> m_waitQueue;
        private readonly ConcurrentQueue<WaitDataAsync<T>> m_waitQueueAsync;
        private int m_currentSign;
        private int m_maxSign = int.MaxValue;
        private int m_minSign = int.MinValue;

        /// <summary>
        /// 初始化WaitHandle池。
        /// </summary>
        /// <remarks>
        /// 在构造函数中，初始化了四个并发集合，用于管理和存储等待数据。
        /// 这些集合分别用于同步和异步操作的等待数据，以及它们之间的转换。
        /// </remarks>
        public WaitHandlePool()
        {
            this.m_waitDic = new ConcurrentDictionary<int, WaitData<T>>();
            this.m_waitDicAsync = new ConcurrentDictionary<int, WaitDataAsync<T>>();
            this.m_waitQueue = new ConcurrentQueue<WaitData<T>>();
            this.m_waitQueueAsync = new ConcurrentQueue<WaitDataAsync<T>>();
        }

        /// <summary>
        /// 最大Sign
        /// </summary>
        public int MaxSign { get => this.m_maxSign; set => this.m_maxSign = value; }

        /// <summary>
        /// 最小Sign
        /// </summary>
        public int MinSign { get => this.m_minSign; set => this.m_minSign = value; }

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
            if (waitData.WaitResult == null)
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
        /// 获取同步等待数据对象，并为其设置结果。
        /// </summary>
        /// <param name="result">要设置给等待数据对象的结果。</param>
        /// <param name="autoSign">是否自动签名，默认为true。</param>
        /// <returns>初始化后的等待数据对象。</returns>
        public WaitData<T> GetWaitData(T result, bool autoSign = true)
        {
            // 尝试从同步等待队列中取出一个等待数据对象
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                // 如果自动签名开启，则为结果对象设置签名
                if (autoSign)
                {
                    result.Sign = this.GetSign();
                }
                // 设置等待数据对象的结果
                waitData.SetResult(result);
                // 将结果对象的签名和等待数据对象添加到字典中
                this.m_waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            // 如果队列中没有可取出的等待数据对象，则新建一个
            waitData = new WaitData<T>();
            // 如果自动签名开启，则为结果对象设置签名
            if (autoSign)
            {
                result.Sign = this.GetSign();
            }
            // 设置等待数据对象的结果
            waitData.SetResult(result);
            // 将结果对象的签名和等待数据对象添加到字典中
            this.m_waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 获取同步等待数据对象，并为其设置默认结果。
        /// </summary>
        /// <param name="sign">返回签名。</param>
        /// <returns>初始化后的等待数据对象。</returns>
        public WaitData<T> GetWaitData(out int sign)
        {
            // 尝试从同步等待队列中取出一个等待数据对象
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                // 生成签名
                sign = this.GetSign();
                // 设置等待数据对象的默认结果
                waitData.SetResult(default);
                // 将签名和等待数据对象添加到字典中
                this.m_waitDic.TryAdd(sign, waitData);
                return waitData;
            }

            // 如果队列中没有可取出的等待数据对象，则新建一个
            waitData = new WaitData<T>();
            // 生成签名
            sign = this.GetSign();
            // 设置等待数据对象的默认结果
            waitData.SetResult(default);
            // 将签名和等待数据对象添加到字典中
            this.m_waitDic.TryAdd(sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 获取异步等待数据对象，并为其设置结果。
        /// </summary>
        /// <param name="result">要设置给等待数据对象的结果。</param>
        /// <param name="autoSign">是否自动签名，默认为true。</param>
        /// <returns>初始化后的等待数据对象。</returns>
        public WaitDataAsync<T> GetWaitDataAsync(T result, bool autoSign = true)
        {
            // 尝试从异步等待队列中取出一个等待数据对象
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                // 如果自动签名开启，则为结果对象设置签名
                if (autoSign)
                {
                    result.Sign = this.GetSign();
                }
                // 设置等待数据对象的结果
                waitData.SetResult(result);
                // 将结果对象的签名和等待数据对象添加到字典中
                this.m_waitDicAsync.TryAdd(result.Sign, waitData);
                return waitData;
            }

            // 如果队列中没有可取出的等待数据对象，则新建一个
            waitData = new WaitDataAsync<T>();
            // 如果自动签名开启，则为结果对象设置签名
            if (autoSign)
            {
                result.Sign = this.GetSign();
            }
            // 设置等待数据对象的结果
            waitData.SetResult(result);
            // 将结果对象的签名和等待数据对象添加到字典中
            this.m_waitDicAsync.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 获取异步等待数据对象，并为其设置默认结果。
        /// </summary>
        /// <param name="sign">返回签名。</param>
        /// <returns>初始化后的等待数据对象。</returns>
        public WaitDataAsync<T> GetWaitDataAsync(out int sign)
        {
            // 尝试从异步等待队列中取出一个等待数据对象
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                // 生成签名
                sign = this.GetSign();
                // 设置等待数据对象的默认结果
                waitData.SetResult(default);
                // 将签名和等待数据对象添加到字典中
                this.m_waitDicAsync.TryAdd(sign, waitData);
                return waitData;
            }

            // 如果队列中没有可取出的等待数据对象，则新建一个
            waitData = new WaitDataAsync<T>();
            // 生成签名
            sign = this.GetSign();
            // 设置等待数据对象的默认结果
            waitData.SetResult(default);
            // 将签名和等待数据对象添加到字典中
            this.m_waitDicAsync.TryAdd(sign, waitData);
            return waitData;
        }

       
               /// <summary>
        /// 根据标志设置异步等待数据为运行状态。
        /// </summary>
        /// <param name="sign">操作的标志。</param>
        /// <returns>如果找到并设置等待数据，则返回true；否则返回false。</returns>
        public bool SetRun(int sign)
        {
            // 尝试从异步等待数据字典中获取并设置等待数据
            if (this.m_waitDicAsync.TryGetValue(sign, out var waitDataAsync))
            {
                waitDataAsync.Set();
                return true;
            }

            // 尝试从同步等待数据字典中获取并设置等待数据
            if (this.m_waitDic.TryGetValue(sign, out var waitData))
            {
                waitData.Set();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据标志和结果对象设置等待数据为运行状态。
        /// </summary>
        /// <param name="sign">操作的标志。</param>
        /// <param name="waitResult">等待的结果对象。</param>
        /// <returns>如果找到并设置等待数据，则返回true；否则返回false。</returns>
        public bool SetRun(int sign, T waitResult)
        {
            // 尝试从异步等待数据字典中获取并设置等待数据
            if (this.m_waitDicAsync.TryGetValue(sign, out var waitDataAsync))
            {
                waitDataAsync.Set(waitResult);
                return true;
            }
            // 尝试从同步等待数据字典中获取并设置等待数据
            if (this.m_waitDic.TryGetValue(sign, out var waitData))
            {
                waitData.Set(waitResult);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据结果对象的标志设置异步等待数据为运行状态。
        /// </summary>
        /// <param name="waitResult">等待的结果对象，包含标志和数据。</param>
        /// <returns>如果找到并设置等待数据，则返回true；否则返回false。</returns>
        public bool SetRun(T waitResult)
        {
            // 尝试从异步等待数据字典中获取并设置等待数据
            if (this.m_waitDicAsync.TryGetValue(waitResult.Sign, out var waitDataAsync))
            {
                waitDataAsync.Set(waitResult);
                return true;
            }

            // 尝试从同步等待数据字典中获取并设置等待数据
            if (this.m_waitDic.TryGetValue(waitResult.Sign, out var waitData))
            {
                waitData.Set(waitResult);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试获取指定标志的同步等待数据。
        /// </summary>
        /// <param name="sign">操作的标志。</param>
        /// <param name="waitData">获取到的等待数据。</param>
        /// <returns>如果找到等待数据，则返回true；否则返回false。</returns>
        public bool TryGetData(int sign, out WaitData<T> waitData)
        {
            return this.m_waitDic.TryGetValue(sign, out waitData);
        }

        /// <summary>
        /// 尝试获取指定标志的异步等待数据。
        /// </summary>
        /// <param name="sign">操作的标志。</param>
        /// <param name="waitDataAsync">获取到的异步等待数据。</param>
        /// <returns>如果找到异步等待数据，则返回true；否则返回false。</returns>
        public bool TryGetDataAsync(int sign, out WaitDataAsync<T> waitDataAsync)
        {
            return this.m_waitDicAsync.TryGetValue(sign, out waitDataAsync);
        }
        
         /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
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
            }

            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetSign()
        {
            Interlocked.CompareExchange(ref this.m_currentSign, 0, this.m_maxSign);
            return Interlocked.Increment(ref this.m_currentSign);
        }
    }
}