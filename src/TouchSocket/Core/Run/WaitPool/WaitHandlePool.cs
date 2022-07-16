//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;

namespace TouchSocket.Core.Run
{
    /// <summary>
    /// 等待处理数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitHandlePool<T> : IDisposable where T : IWaitResult
    {
        private SnowflakeIDGenerator idGenerator;
        private ConcurrentDictionary<long, WaitData<T>> waitDic;
        private ConcurrentQueue<WaitData<T>> waitQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitHandlePool()
        {
            this.waitDic = new ConcurrentDictionary<long, WaitData<T>>();
            this.waitQueue = new ConcurrentQueue<WaitData<T>>();
            this.idGenerator = new SnowflakeIDGenerator(4);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="waitData"></param>
        public void Destroy(WaitData<T> waitData)
        {
            if (waitData.DisposedValue)
            {
                throw new ObjectDisposedException(nameof(waitData));
            }
            if (this.waitDic.TryRemove(waitData.WaitResult.Sign, out _))
            {
                waitData.Reset();
                this.waitQueue.Enqueue(waitData);
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            foreach (var item in this.waitDic.Values)
            {
                item.Dispose();
            }
            foreach (var item in this.waitQueue)
            {
                item.Dispose();
            }
            this.waitDic.Clear();

            this.waitQueue.Clear();
        }

        /// <summary>
        ///  获取一个可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitData<T> GetWaitData(T result, bool autoSign = true)
        {
            WaitData<T> waitData;
            if (this.waitQueue.TryDequeue(out waitData))
            {
                if (autoSign)
                {
                    result.Sign = this.idGenerator.NextID();
                }
                waitData.SetResult(result);
                this.waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            if (autoSign)
            {
                result.Sign = this.idGenerator.NextID();
            }
            waitData.SetResult(result);
            this.waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        public void SetRun(long sign)
        {
            WaitData<T> waitData;
            if (this.waitDic.TryGetValue(sign, out waitData))
            {
                waitData.Set();
            }
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="waitResult"></param>
        public void SetRun(long sign, T waitResult)
        {
            WaitData<T> waitData;
            if (this.waitDic.TryGetValue(sign, out waitData))
            {
                waitData.Set(waitResult);
            }
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="waitResult"></param>
        public void SetRun(T waitResult)
        {
            WaitData<T> waitData;
            if (this.waitDic.TryGetValue(waitResult.Sign, out waitData))
            {
                waitData.Set(waitResult);
            }
        }
    }
}