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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 表示可连接的服务器基类
/// </summary>
public abstract class ConnectableService : ServiceBase, IConnectableService
{
    private Func<string> m_getDefaultNewId;

    private int m_maxCount;
    private int m_nextId;

    /// <summary>
    /// 表示可连接的服务器基类
    /// </summary>
    protected ConnectableService()
    {
        this.m_getDefaultNewId = this.GetDefaultNewId;
    }

    /// <inheritdoc/>
    public abstract int Count { get; }

    /// <inheritdoc/>
    public int MaxCount => this.m_maxCount;

    /// <inheritdoc/>
    public abstract Task ClearAsync();

    /// <inheritdoc/>
    public abstract bool ClientExists(string id);

    IEnumerable<IClient> IConnectableService.GetClients()
    {
        return this.GetClients();
    }

    /// <inheritdoc/>
    public abstract IEnumerable<string> GetIds();

    /// <inheritdoc/>
    public abstract Task ResetIdAsync(string sourceId, string targetId);

    /// <inheritdoc cref="IConnectableService.GetClients"/>
    protected abstract IEnumerable<IClient> GetClients();


    /// <summary>
    /// 尝试获取下一个新的标识符。
    /// </summary>
    /// <returns>返回新的标识符，如果内部方法调用失败，则返回默认的新标识符。</returns>
    protected virtual string GetNextNewId()
    {
        try
        {
            // 尝试调用内部方法获取新的标识符。
            return this.m_getDefaultNewId.Invoke();
        }
        catch (Exception ex)
        {
            // 如果调用过程中发生异常，记录异常信息。
            this.Logger?.Exception(this,ex);
        }
        // 如果调用内部方法失败，则使用此方法作为回退，返回一个默认的新标识符。
        return this.GetDefaultNewId();
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        if (config.GetValue(TouchSocketConfigExtension.GetDefaultNewIdProperty) is Func<string> fun)
        {
            this.m_getDefaultNewId = fun;
        }
        this.m_maxCount = config.GetValue(TouchSocketConfigExtension.MaxCountProperty);

        base.LoadConfig(config);
    }

    private string GetDefaultNewId()
    {
        return BitConverter.ToString(BitConverter.GetBytes(Interlocked.Increment(ref this.m_nextId)));
    }
}