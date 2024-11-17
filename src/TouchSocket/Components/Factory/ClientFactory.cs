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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 客户端工厂的基类，用于创建特定类型的客户端对象。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须实现IClient接口。</typeparam>
    public abstract class ClientFactory<TClient> : DependencyObject where TClient : IClient
    {
        private readonly ConcurrentList<TClient> m_createdClients = new ConcurrentList<TClient>();
        private readonly ConcurrentQueue<TClient> m_freeClients = new ConcurrentQueue<TClient>();
        private readonly SingleTimer m_singleTimer;

        /// <summary>
        /// 客户端工厂类的构造函数。
        /// </summary>
        public ClientFactory()
        {
            // 初始化一个单例计时器，每1000毫秒执行一次指定的回调方法。
            this.m_singleTimer = new SingleTimer(1000, () =>
            {
                // 创建一个临时客户端列表，用于存储需要移除的客户端。
                var list = new List<TClient>();

                // 移除所有不再活跃的客户端。
                this.m_createdClients.RemoveAll(a =>
                {
                    // 如果客户端不再活跃，则移除并处理该客户端。
                    if (!this.IsAlive(a))
                    {
                        this.DisposeClient(a);
                        return true;
                    }
                    return false;
                });

                // 注释掉的代码块，用于在客户端数量不足最小值时新增客户端。
                //if (this.CreatedClients.Count < this.MinCount)
                //{
                //    using (this.GetClient())
                //    {
                //    }
                //}
            });
        }

        #region 属性

        /// <summary>
        /// 获取可用的客户端数量。
        /// <para>
        /// 该值指示了当前空闲的客户端数量和未创建的客户端数量。
        /// </para>
        /// </summary>
        /// <returns></returns>
        public int AvailableCount => Math.Max(0, this.MaxCount - this.CreatedClients.Count) + this.FreeClients.Count;

        /// <summary>
        /// 获取已经创建的客户端数量。
        /// </summary>
        public int CreatedCount => this.CreatedClients.Count;

        /// <summary>
        /// 获取空闲的客户端数量。
        /// </summary>
        public int FreeCount => this.FreeClients.Count;

        /// <summary>
        /// 最大客户端数量。默认10。
        /// </summary>
        public int MaxCount { get; set; } = 10;

        /// <summary>
        /// 池中维护的最小客户端数量。默认0。
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// 已创建的客户端安全列表，一般不要直接操作。
        /// </summary>
        protected IReadOnlyList<TClient> CreatedClients => this.m_createdClients;

        /// <summary>
        /// 空闲客户端的安全队列，一般不要直接操作。
        /// </summary>
        protected ConcurrentQueue<TClient> FreeClients => this.m_freeClients;

        #endregion 属性

        /// <summary>
        /// 清理池中的所有客户端。
        /// </summary>
        /// <returns>被清理的客户端数量。</returns>
        public virtual int Clear()
        {
            // 初始化客户端计数器
            var count = 0;
            // 遍历所有已创建的客户端
            foreach (var item in this.CreatedClients)
            {
                // 增加客户端计数
                count++;
                // 处置当前客户端
                this.DisposeClient(item);
            }
            // 清空空闲客户端集合
            this.FreeClients.Clear();
            // 返回被清理的客户端数量
            return count;
        }

        /// <summary>
        /// 释放客户端最后的调用。
        /// </summary>
        /// <param name="client">待释放的客户端实例。</param>
        public virtual void DisposeClient(TClient client)
        {
            // 安全释放客户端资源
            client.SafeDispose();

            // 从已创建的客户端列表中移除该客户端
            this.m_createdClients.Remove(client);
        }

        #region GetClient

        /// <summary>
        /// 获取用于传输的客户端结果。可以支持<see cref="IDisposable"/>。
        /// </summary>
        /// <param name="waitTime">等待时间，超过此时间则取消获取客户端的操作。</param>
        /// <returns>返回一个<see cref="ClientFactoryResult{TClient}"/>对象，包含租用的客户端和归还客户端的方法。</returns>
        public virtual async ValueTask<ClientFactoryResult<TClient>> GetClient(TimeSpan waitTime)
        {
            // 租用客户端，并配置不等待主线程
            return new ClientFactoryResult<TClient>(await this.RentClient(waitTime).ConfigureAwait(false), this.ReturnClient);
        }

        /// <summary>
        /// 获取一个指定客户端，默认情况下等待1秒。
        /// </summary>
        /// <returns>返回一个<see cref="ClientFactoryResult{TClient}"/>对象，包含租用的客户端和归还客户端的方法。</returns>
        public ValueTask<ClientFactoryResult<TClient>> GetClient()
        {
            // 使用默认等待时间1秒来获取客户端
            return this.GetClient(TimeSpan.FromSeconds(1));
        }

        #endregion GetClient

        /// <summary>
        /// 判断客户端是不是存活状态。
        /// </summary>
        /// <param name="client">要判断的客户端对象。</param>
        /// <returns>返回一个布尔值，表示客户端的存活状态。</returns>
        public abstract bool IsAlive(TClient client);

        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <returns>返回一个异步任务，该任务结果为创建的客户端对象。</returns>
        protected abstract Task<TClient> CreateClient();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_singleTimer.SafeDispose();
                this.Clear();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 租赁客户端
        /// </summary>
        /// <param name="waitTime">等待时间</param>
        /// <returns>客户端实例</returns>
        protected virtual async ValueTask<TClient> RentClient(TimeSpan waitTime)
        {
            // 从空闲客户端队列中尝试取出一个客户端
            while (this.FreeClients.TryDequeue(out var client))
            {
                // 如果客户端仍然存活，则返回该客户端
                if (this.IsAlive(client))
                {
                    return client;
                }
            }

            // 如果已创建的客户端数量超过最大数量
            if (this.CreatedClients.Count > this.MaxCount)
            {
                // 等待指定时间，然后递归尝试租赁客户端
                if (SpinWait.SpinUntil(this.Wait, waitTime))
                {
                    return await this.RentClient(waitTime).ConfigureAwait(false);
                }
            }

            // 创建一个新的客户端
            var clientRes = await this.CreateClient().ConfigureAwait(false);
            // 将新创建的客户端添加到已创建的客户端列表中
            this.m_createdClients.Add(clientRes);
            // 返回新创建的客户端
            return clientRes;
        }

        /// <summary>
        /// 放回使用完成的客户端
        /// </summary>
        /// <param name="client">客户端实例</param>
        protected virtual void ReturnClient(TClient client)
        {
            // 如果客户端不再存活，则将其销毁
            if (!this.IsAlive(client))
            {
                this.DisposeClient(client);
                return;
            }
            // 如果空闲客户端的数量小于最大数量，则将其放回空闲队列
            if (this.FreeClients.Count < this.MaxCount)
            {
                this.FreeClients.Enqueue(client);
            }
            else
            {
                // 如果空闲客户端的数量已经达到最大数量，则销毁客户端
                this.DisposeClient(client);
            }
        }

        private bool Wait()
        {
            return !this.FreeClients.IsEmpty;
        }
    }
}