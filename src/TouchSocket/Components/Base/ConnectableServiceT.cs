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

namespace TouchSocket.Sockets;

/// <summary>
/// ConnectableService 类为实现与客户端的连接提供了一个框架。
/// 这个抽象类以泛型的形式定义，允许继承该类的服务处理特定类型的客户端。
/// </summary>
/// <typeparam name="TClient">客户端类型，必须实现 IClient 和 IIdClient 接口。</typeparam>
public abstract class ConnectableService<TClient> : ConnectableService, IConnectableService<TClient> where TClient : IClient, IIdClient
{
    /// <summary>
    /// 客户端集合属性，返回一个客户端集合对象，该对象管理所有连接的客户端。
    /// </summary>
    /// <value>一个<see cref="IClientCollection{TClient}"/>泛型接口，用于管理客户端实例。</value>
    public abstract IClientCollection<TClient> Clients { get; }

    /// <summary>
    /// 客户端实例初始化完成。
    /// </summary>
    /// <param name="client">初始化完成的客户端实例。</param>
    protected virtual void ClientInitialized(TClient client)
    {
    }

    /// <summary>
    /// 获取所有客户端实例的集合。
    /// </summary>
    /// <returns>一个 IClient 接口的集合，包含所有客户端实例。</returns>
    protected sealed override IEnumerable<IClient> GetClients()
    {
        // 将 Clients 集合转换为 IClient 接口的集合并返回。
        return this.Clients.ToList().Cast<IClient>();
    }

    /// <summary>
    /// 创建一个新的客户端实例。
    /// </summary>
    /// <returns>一个 TClient 类型的新的客户端实例。</returns>
    protected abstract TClient NewClient();
}