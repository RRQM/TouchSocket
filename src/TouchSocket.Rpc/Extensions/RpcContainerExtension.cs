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
using System.Diagnostics.CodeAnalysis;
using TouchSocket.Core;
using static System.Collections.Specialized.BitVector32;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TouchSocket.Rpc;

/// <summary>
/// ContainerExtension
/// </summary>
public static partial class RpcContainerExtension
{
    /// <summary>
    /// 向容器中添加<see cref="RpcStore"/>。
    /// </summary>
    /// <param name="registrator"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IRegistrator AddRpcStore(this IRegistrator registrator, Action<RpcStore> action)
    {
        AddRpcServerProvider(registrator);
        var rpcStore = new RpcStore(registrator);
        action.Invoke(rpcStore);
        registrator.RegisterSingleton(rpcStore);
        return registrator;
    }

    /// <summary>
    /// 添加Rpc服务器提供者
    /// </summary>
    /// <typeparam name="TRpcServerProvider"></typeparam>
    /// <param name="registrator"></param>
    /// <returns></returns>
    public static IRegistrator AddRpcServerProvider<[DynamicallyAccessedMembers(CoreContainerExtension.DynamicallyAccessed)] TRpcServerProvider>(this IRegistrator registrator) where TRpcServerProvider : class, IRpcServerProvider
    {
        registrator.RegisterSingleton<IRpcServerProvider, TRpcServerProvider>();
        return registrator;
    }

    /// <summary>
    /// 添加默认Rpc服务器提供者
    /// </summary>
    /// <param name="registrator"></param>
    /// <returns></returns>
    public static IRegistrator AddRpcServerProvider(this IRegistrator registrator)
    {
        return AddRpcServerProvider<InternalRpcServerProvider>(registrator);
    }
}