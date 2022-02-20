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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RRQMCore.Pool
{
    /// <summary>
    /// 对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IObjectPool where T : IPoolObject
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        public ObjectPool(int capacity)
        {
            this.Capacity = capacity;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ObjectPool()
        {
        }

        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        private bool autoCreate = true;

        /// <summary>
        /// 是否自动生成
        /// </summary>
        public bool AutoCreate
        {
            get { return this.autoCreate; }
            set { this.autoCreate = value; }
        }

        /// <summary>
        /// 对象池容量
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// 可使用（创建）数量
        /// </summary>
        public int FreeSize
        { get { return this.freeSize; } }

        private int freeSize;

        /// <summary>
        /// 清除池中所有对象
        /// </summary>
        public void Clear()
        {
            while (this.queue.TryDequeue(out _))
            {
            }
        }

        /// <summary>
        /// 获取对象T
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            T t;
            if (this.queue.TryDequeue(out t))
            {
                t.Recreate();
                t.NewCreate = false;
                Interlocked.Decrement(ref this.freeSize);
                return t;
            }
            if (this.autoCreate)
            {
                t = (T)Activator.CreateInstance(typeof(T));
                t.Create();
                t.NewCreate = true;
            }
            return t;
        }

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <returns></returns>
        public T[] GetAllObject()
        {
            List<T> ts = new List<T>();
            while (this.queue.TryDequeue(out T t))
            {
                ts.Add(t);
            }
            return ts.ToArray();
        }

        /// <summary>
        /// 预获取
        /// </summary>
        /// <returns></returns>
        public T PreviewGetObject()
        {
            T t;
            this.queue.TryPeek(out t);
            return t;
        }

        /// <summary>
        /// 注销对象
        /// </summary>
        /// <param name="t"></param>
        public void DestroyObject(T t)
        {
            t.Destroy();
            if (this.freeSize < this.Capacity)
            {
                Interlocked.Increment(ref this.freeSize);
                this.queue.Enqueue(t);
            }
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        public void Dispose()
        {
            this.Clear();
        }
    }
}