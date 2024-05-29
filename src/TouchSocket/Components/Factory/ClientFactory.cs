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
    /// Client工厂
    /// </summary>
    public abstract class ClientFactory<TClient> : DependencyObject where TClient : IClient
    {
        private readonly ConcurrentList<TClient> m_createdClients = new ConcurrentList<TClient>();
        private readonly ConcurrentQueue<TClient> m_freeClients = new ConcurrentQueue<TClient>();
        private readonly SingleTimer m_singleTimer;

        public ClientFactory()
        {
            this.m_singleTimer = new SingleTimer(1000, () =>
            {
                var list = new List<TClient>();

                this.m_createdClients.RemoveAll(a =>
                {
                    if (!this.IsAlive(a))
                    {
                        this.DisposeClient(a);
                        return true;
                    }
                    return false;
                });

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
        /// <returns></returns>
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
        /// <returns></returns>
        public virtual int Clear()
        {
            var count = 0;
            foreach (var item in this.CreatedClients)
            {
                count++;
                this.DisposeClient(item);
            }
            this.FreeClients.Clear();
            return count;
        }

        /// <summary>
        /// 释放客户端最后的调用。
        /// </summary>
        /// <param name="client"></param>
        public virtual void DisposeClient(TClient client)
        {
            client.SafeDispose();
            this.m_createdClients.Remove(client);
        }

        #region GetClient

        /// <summary>
        /// 获取用于传输的客户端结果。可以支持<see cref="IDisposable"/>。
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public virtual async ValueTask<ClientFactoryResult<TClient>> GetClient(TimeSpan waitTime)
        {
            return new ClientFactoryResult<TClient>(await this.RentClient(waitTime).ConfigureAwait(false), this.ReturnClient);
        }

        /// <summary>
        /// 获取一个指定客户端，默认情况下等待1秒。
        /// </summary>
        /// <returns></returns>
        public ValueTask<ClientFactoryResult<TClient>> GetClient()
        {
            return this.GetClient(TimeSpan.FromSeconds(1));
        }

        #endregion GetClient

        /// <summary>
        /// 判断客户端是不是存活状态。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public abstract bool IsAlive(TClient client);

        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <returns></returns>
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
        /// <param name="waitTime"></param>
        /// <returns></returns>
        protected virtual async ValueTask<TClient> RentClient(TimeSpan waitTime)
        {
            while (this.FreeClients.TryDequeue(out var client))
            {
                if (this.IsAlive(client))
                {
                    return client;
                }
            }

            if (this.CreatedClients.Count > this.MaxCount)
            {
                if (SpinWait.SpinUntil(this.Wait, waitTime))
                {
                    return await this.RentClient(waitTime).ConfigureAwait(false);
                }
            }

            var clientRes = await this.CreateClient().ConfigureFalseAwait();
            this.m_createdClients.Add(clientRes);
            return clientRes;
        }

        /// <summary>
        /// 放回使用完成的客户端
        /// </summary>
        /// <param name="client"></param>
        protected virtual void ReturnClient(TClient client)
        {
            if (!this.IsAlive(client))
            {
                this.DisposeClient(client);
                return;
            }
            if (this.FreeClients.Count < this.MaxCount)
            {
                this.FreeClients.Enqueue(client);
            }
            else
            {
                this.DisposeClient(client);
            }
        }

        private bool Wait()
        {
            return !this.FreeClients.IsEmpty;
        }
    }
}