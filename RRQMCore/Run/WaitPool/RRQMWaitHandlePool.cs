//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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

using RRQMCore.Extensions;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RRQMCore.Run
{
    /// <summary>
    /// 等待处理数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RRQMWaitHandlePool<T> : IDisposable where T : IWaitResult
    {
        private int signCount;

        private ConcurrentDictionary<int, WaitData<T>> waitDic;

        private ConcurrentQueue<WaitData<T>> waitQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMWaitHandlePool()
        {
            this.waitDic = new ConcurrentDictionary<int, WaitData<T>>();
            this.waitQueue = new ConcurrentQueue<WaitData<T>>();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="waitData"></param>
        public void Destroy(WaitData<T> waitData)
        {
            if (waitData._dispose)
            {
                throw new RRQMException("waitData已销毁");
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
        /// 获取一个可等待对象
        /// </summary>
        public WaitData<T> GetWaitData(T result)
        {
            if (this.signCount == int.MaxValue)
            {
                this.signCount = 0;
            }
            WaitData<T> waitData;
            if (this.waitQueue.TryDequeue(out waitData))
            {
                result.Sign = Interlocked.Increment(ref this.signCount);
                waitData.SetResult(result);
                this.waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            result.Sign = Interlocked.Increment(ref this.signCount);
            waitData.SetResult(result);
            this.waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        public void SetRun(int sign)
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
        public void SetRun(int sign, T waitResult)
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