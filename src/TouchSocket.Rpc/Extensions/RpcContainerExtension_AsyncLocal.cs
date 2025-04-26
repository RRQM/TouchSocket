// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

#if AsyncLocal
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc;
/// <summary>
/// 提供扩展方法以注册和管理RPC调用上下文访问器。
/// </summary>
public static partial class RpcContainerExtension
{
    #region RpcCallContextAccessor
    /// <summary>
    /// 将指定类型的RPC调用上下文访问器注册为单例。
    /// </summary>
    /// <typeparam name="TRpcCallContextAccessor">实现 <see cref="IRpcCallContextAccessor"/> 的类型。</typeparam>
    /// <param name="registrator">用于注册依赖项的 <see cref="IRegistrator"/> 实例。</param>
    /// <returns>返回 <see cref="IRegistrator"/> 实例以支持链式调用。</returns>
    public static IRegistrator AddRpcCallContextAccessor<TRpcCallContextAccessor>(this IRegistrator registrator)
       where TRpcCallContextAccessor : class, IRpcCallContextAccessor
    {
        registrator.RegisterSingleton<IRpcCallContextAccessor, TRpcCallContextAccessor>();
        return registrator;
    }

    /// <summary>
    /// 将默认的RPC调用上下文访问器注册为单例。
    /// </summary>
    /// <param name="registrator">用于注册依赖项的 <see cref="IRegistrator"/> 实例。</param>
    /// <returns>返回 <see cref="IRegistrator"/> 实例以支持链式调用。</returns>
    public static IRegistrator AddRpcCallContextAccessor(this IRegistrator registrator)
    {
        return AddRpcCallContextAccessor<RpcCallContextAccessor>(registrator);
    }
    #endregion
}

#endif
