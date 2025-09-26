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
using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Rpc;

/// <summary>
/// Rpc仓库
/// </summary>
public sealed class RpcStore
{
    private readonly IRegistrator m_registrator;
    private readonly ConcurrentDictionary<Type, List<RpcMethod>> m_serverTypes = new ConcurrentDictionary<Type, List<RpcMethod>>();

    /// <summary>
    /// 实例化一个Rpc仓库。
    /// </summary>
    public RpcStore(IRegistrator registrator)
    {
        this.m_registrator = registrator;
    }

    /// <summary>
    /// 服务类型
    /// </summary>
    public Type[] ServerTypes => this.m_serverTypes.Keys.ToArray();

    /// <summary>
    /// 全局筛选器
    /// </summary>
    public List<Type> Filters => this.m_filters;

    /// <summary>
    /// 获取所有已注册的函数。
    /// </summary>
    public RpcMethod[] GetAllMethods()
    {
        var methods = new List<RpcMethod>();
        foreach (var item in this.m_serverTypes.Values)
        {
            methods.AddRange(item);
        }

        return methods.ToArray();
    }

    /// <summary>
    /// 本地获取代理
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="attributeTypes"></param>
    /// <returns></returns>
    public string GetProxyCodes(string @namespace, params Type[] attributeTypes)
    {
        var cellCodes = this.GetProxyInfo(attributeTypes);
        return CodeGenerator.ConvertToCode(@namespace, cellCodes);
    }

    /// <summary>
    /// 获取生成的代理
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="namespace"></param>
    /// <returns></returns>
    public string GetProxyCodes<TAttribute>(string @namespace) where TAttribute : RpcAttribute
    {
        var cellCodes = this.GetProxyInfo(new Type[] { typeof(TAttribute) });
        return CodeGenerator.ConvertToCode(@namespace, cellCodes);
    }

    /// <summary>
    /// 从本地获取代理
    /// </summary>
    /// <param name="attributeType"></param>
    /// <returns></returns>
    public ServerCellCode[] GetProxyInfo(Type[] attributeType)
    {
        var codes = new List<ServerCellCode>();

        foreach (var attribute in attributeType)
        {
            foreach (var item in this.m_serverTypes.Keys)
            {
                var serverCellCode = CodeGenerator.Generator(item, attribute);
                codes.Add(serverCellCode);
            }
        }
        return codes.ToArray();
    }

    /// <summary>
    /// 获取服务类型对应的服务方法。
    /// </summary>
    /// <param name="serverType"></param>
    /// <returns></returns>
    public RpcMethod[] GetServerRpcMethods(Type serverType)
    {
        return this.m_serverTypes[serverType].ToArray();
    }

    #region 全局筛选器

    private readonly List<Type> m_filters = new();

    /// <summary>
    /// 添加全局筛选器
    /// </summary>
    /// <typeparam name="TFilter"></typeparam>
    public void AddFilter<TFilter>() where TFilter : class, IRpcActionFilter
    {
        var filterType = typeof(TFilter);
        this.AddFilter(filterType);
    }

    /// <summary>
    /// 添加全局过滤器
    /// </summary>
    public void AddFilter(Type filterType)
    {
        if (!typeof(IRpcActionFilter).IsAssignableFrom(filterType))
        {
            throw new RpcException($"注册类型必须与{nameof(IRpcActionFilter)}有继承关系");
        }

        if (!this.m_filters.Any(s => s == filterType))
        {
            this.m_filters.Add(filterType);
            this.m_registrator.RegisterTransient(filterType);
        }
    }


    #endregion

    #region 注册

    /// <summary>
    /// 注册为单例服务
    /// </summary>
    /// <param name="serverFromType"></param>
    /// <param name="rpcServer"></param>
    /// <returns></returns>
    public void RegisterServer(Type serverFromType, IRpcServer rpcServer)
    {
        if (!typeof(IRpcServer).IsAssignableFrom(serverFromType))
        {
            throw new RpcException($"注册类型必须与{nameof(IRpcServer)}有继承关系");
        }

        if (!serverFromType.IsAssignableFrom(rpcServer.GetType()))
        {
            throw new RpcException("实例类型必须与注册类型有继承关系。");
        }

        foreach (var item in this.m_serverTypes.Keys)
        {
            if (item.FullName == serverFromType.FullName)
            {
                return;
            }
        }

        var rpcMethods = CodeGenerator.GetRpcMethods(serverFromType, rpcServer.GetType());

        this.m_serverTypes.AddOrUpdate(serverFromType, new List<RpcMethod>(rpcMethods));
        this.m_registrator.RegisterSingleton(serverFromType, rpcServer);
    }

    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="serverFromType"></param>
    /// <param name="serverToType"></param>
    /// <returns></returns>
    public void RegisterServer(Type serverFromType, [DynamicallyAccessedMembers(RpcStoreExtension.DynamicallyAccessed)] Type serverToType)
    {
        if (!typeof(IRpcServer).IsAssignableFrom(serverFromType))
        {
            throw new RpcException($"注册类型必须与{nameof(IRpcServer)}有继承关系");
        }

        if (!serverFromType.IsAssignableFrom(serverToType))
        {
            throw new RpcException("实例类型必须与注册类型有继承关系。");
        }

        foreach (var item in this.m_serverTypes.Keys)
        {
            if (item == serverFromType)
            {
                return;
            }
        }

        if (typeof(ITransientRpcServer).IsAssignableFrom(serverFromType))
        {
            this.m_registrator.RegisterTransient(serverFromType, serverToType);
        }
        else if (typeof(IScopedRpcServer).IsAssignableFrom(serverFromType))
        {
            this.m_registrator.Register(new DependencyDescriptor(serverFromType, serverToType, Lifetime.Scoped));
        }
        else
        {
            this.m_registrator.RegisterSingleton(serverFromType, serverToType);
        }
        var rpcMethods = CodeGenerator.GetRpcMethods(serverFromType, serverToType);

        this.m_serverTypes.AddOrUpdate(serverFromType, new List<RpcMethod>(rpcMethods));
    }

    #endregion 注册
}