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

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

using TouchSocket.Resources;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// 定义了用于简化DMTP RPC Actor操作的扩展方法。
/// </summary>
public static class DmtpRpcActorExtension
{
    #region DependencyProperty

    ///// <summary>
    ///// DmtpRpcActor
    ///// </summary>
    //public static readonly DependencyProperty<IDmtpRpcActor> DmtpRpcActorProperty =
    //    new("DmtpRpcActor", default);

    #endregion DependencyProperty

    /// <summary>
    /// 新创建一个直接向目标地址请求的<see cref="IDmtpRpcActor"/>客户端。
    /// </summary>
    /// <param name="client">要为其创建目标DMTP RPC演员的客户端。</param>
    /// <param name="targetId">目标地址的标识符。</param>
    /// <returns>返回一个新的<see cref="IDmtpRpcActor"/>实例，该实例能够直接向指定目标地址发起请求。</returns>
    public static IDmtpRpcActor CreateTargetDmtpRpcActor(this IDmtpActorObject client, string targetId)
    {
        // 使用指定的目标ID和当前客户端的DMTP RPC演员创建一个新的目标DMTP RPC演员。
        return new TargetDmtpRpcActor(targetId, client.GetDmtpRpcActor());
    }

    /// <summary>
    /// 从<see cref="DmtpActor"/>中获取<see cref="IDmtpRpcActor"/>
    /// </summary>
    /// <param name="dmtpActor">要从中获取<see cref="IDmtpRpcActor"/>的<see cref="DmtpActor"/></param>
    /// <returns>返回获取到的<see cref="IDmtpRpcActor"/></returns>
    public static IDmtpRpcActor GetDmtpRpcActor(this IDmtpActor dmtpActor)
    {
        return dmtpActor.GetActor<DmtpRpcActor>();
    }

    /// <summary>
    /// 从<see cref="IDmtpActorObject"/>中获取<see cref="IDmtpRpcActor"/>，以实现Rpc调用功能。
    /// </summary>
    /// <param name="client">要获取<see cref="IDmtpRpcActor"/>的<see cref="IDmtpActorObject"/>对象。</param>
    /// <returns>返回<see cref="IDmtpRpcActor"/>对象。</returns>
    /// <exception cref="ArgumentNullException">如果<see cref="IDmtpRpcActor"/>对象为<see langword="null"/>，则抛出此异常。</exception>
    public static IDmtpRpcActor GetDmtpRpcActor(this IDmtpActorObject client)
    {
        var actor = client.DmtpActor.GetDmtpRpcActor();
        ThrowHelper.ThrowArgumentNullExceptionIf(actor, nameof(actor), TouchSocketDmtpResource.DmtpRpcActorArgumentNull);
        return actor;
    }

    /// <summary>
    /// 从<see cref="IDmtpActorObject"/>中获取继承实现<see cref="IDmtpRpcActor"/>的功能件，以实现Rpc调用功能。
    /// </summary>
    /// <param name="client">一个实现了<see cref="IDmtpActorObject"/>接口的对象，该对象中包含了需要获取的RpcActor。</param>
    /// <returns>返回一个继承自<see cref="IDmtpRpcActor"/>接口的实例，类型为TDmtpRpcActor。</returns>
    /// <exception cref="ArgumentNullException">当无法从<paramref name="client"/>中获取到DmtpRpcActor时抛出。</exception>
    public static TDmtpRpcActor GetDmtpRpcActor<TDmtpRpcActor>(this IDmtpActorObject client) where TDmtpRpcActor : IDmtpRpcActor
    {
        var actor = client.DmtpActor.GetDmtpRpcActor();
        ThrowHelper.ThrowArgumentNullExceptionIf(actor, nameof(actor), TouchSocketDmtpResource.DmtpRpcActorArgumentNull);
        return (TDmtpRpcActor)actor;
    }
}