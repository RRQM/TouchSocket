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

namespace TouchSocket.Sockets;

/// <summary>
/// 客户端工厂的基类，用于创建特定类型的客户端对象。
/// </summary>
/// <typeparam name="TClient">客户端类型，必须实现<see cref="IClient"/>接口。</typeparam>
public abstract class ClientFactory<TClient> : DependencyObject where TClient : IClient
{
    private readonly ConcurrentList<TClient> m_createdClients = new ConcurrentList<TClient>();
    private readonly CancellationTokenSource m_cts = new CancellationTokenSource();
    private readonly ConcurrentQueue<TClient> m_freeClients = new ConcurrentQueue<TClient>();

    /// <summary>
    /// 客户端工厂类的构造函数。
    /// </summary>
    public ClientFactory()
    {
        // 启动定时任务但不阻塞构造函数
        _ = this.RunPeriodicAsync();
    }

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
    protected abstract Task<TClient> CreateClient(CancellationToken token);

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_cts.Cancel();
            this.Clear();
        }
        base.SafetyDispose(disposing);
    }

    /// <summary>
    /// 租赁客户端
    /// </summary>
    /// <param name="token"></param>
    /// <returns>客户端实例</returns>
    protected virtual async ValueTask<TClient> RentClient(CancellationToken token)
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
            var startTime = DateTimeOffset.UtcNow;
            while (true)
            {
                // 检查是否超过最大等待时间
                if (DateTimeOffset.UtcNow - startTime > this.MaxWaitTime)
                {
                    // 如果超过最大等待时间，创建一个新的客户端
                    break;
                }
                token.ThrowIfCancellationRequested();
                if (!this.FreeClients.IsEmpty)
                {
                    return await this.RentClient(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }

                await Task.Delay(10, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }

        // 创建一个新的客户端
        var clientRes = await this.CreateClient(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    private async Task ExecuteAsyncTask()
    {
        try
        {
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


            var clients = new List<TClient>();
            while (this.CreatedCount < this.MinCount)
            {
                var client = await this.RentClient(CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                clients.Add(client);
            }

            foreach (var item in clients)
            {
                this.ReturnClient(item);
            }
        }
        catch
        {

        }
    }

    private async Task RunPeriodicAsync()
    {
        while (!this.m_cts.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(EasyTask.ContinueOnCapturedContext); // 每秒执行
            await this.ExecuteAsyncTask().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
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
    /// 获取或设置客户端池等待可用客户端的最大时间。默认1秒。
    /// </summary>
    public TimeSpan MaxWaitTime { get; set; } = TimeSpan.FromSeconds(1);

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

    #region GetClient

    /// <summary>
    /// 获取一个客户端实例，并在使用完成后自动归还到客户端池。
    /// </summary>
    /// <param name="token">用于取消操作的 <see cref="CancellationToken"/>。</param>
    /// <returns>包含客户端实例和归还方法的 <see cref="ClientFactoryResult{TClient}"/>。</returns>
    public virtual async ValueTask<ClientFactoryResult<TClient>> GetClient(CancellationToken token = default)
    {
        // 租用客户端，并配置不等待主线程
        return new ClientFactoryResult<TClient>(await this.RentClient(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext), this.ReturnClient);
    }
    #endregion GetClient
}