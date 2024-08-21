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

using System;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 定义了用于简化DMTP RPC Actor操作的扩展方法。
    /// </summary>
    public static class DmtpRpcActorExtension
    {
        #region DependencyProperty

        /// <summary>
        /// DmtpRpcActor
        /// </summary>
        public static readonly DependencyProperty<IDmtpRpcActor> DmtpRpcActorProperty =
            new("DmtpRpcActor", default);

        #endregion DependencyProperty

        /// <summary>
        /// 新创建一个直接向目标地址请求的<see cref="IRpcClient"/>客户端。
        /// </summary>
        /// <param name="client">要为其创建目标DMTP RPC演员的客户端。</param>
        /// <param name="targetId">目标地址的标识符。</param>
        /// <returns>返回一个新的<see cref="IRpcClient"/>实例，该实例能够直接向指定目标地址发起请求。</returns>
        public static IRpcClient CreateTargetDmtpRpcActor(this IDmtpActorObject client, string targetId)
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
            // 调用GetValue方法从dmtpActor中获取DmtpRpcActorProperty属性值
            return dmtpActor.GetValue(DmtpRpcActorProperty);
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取<see cref="IDmtpRpcActor"/>，以实现Rpc调用功能。
        /// </summary>
        /// <param name="client">要获取<see cref="IDmtpRpcActor"/>的<see cref="IDmtpActorObject"/>对象。</param>
        /// <returns>返回<see cref="IDmtpRpcActor"/>对象。</returns>
        /// <exception cref="ArgumentNullException">如果<see cref="IDmtpRpcActor"/>对象为null，则抛出此异常。</exception>
        public static IDmtpRpcActor GetDmtpRpcActor(this IDmtpActorObject client)
        {
            // 从client的DmtpActor属性中获取DmtpRpcActor对象。
            var dmtpRpcActor = client.DmtpActor.GetDmtpRpcActor();
            // 如果获取的DmtpRpcActor对象为null，则抛出ArgumentNullException异常。
            if (dmtpRpcActor is null)
            {
                throw new ArgumentNullException(nameof(dmtpRpcActor), TouchSocketDmtpResource.DmtpRpcActorArgumentNull);
            }
            // 返回获取到的DmtpRpcActor对象。
            return dmtpRpcActor;
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取继承实现<see cref="IDmtpRpcActor"/>的功能件，以实现Rpc调用功能。
        /// </summary>
        /// <param name="client">一个实现了<see cref="IDmtpActorObject"/>接口的对象，该对象中包含了需要获取的RpcActor。</param>
        /// <returns>返回一个继承自<see cref="IDmtpRpcActor"/>接口的实例，类型为TDmtpRpcActor。</returns>
        /// <exception cref="ArgumentNullException">当无法从<paramref name="client"/>中获取到DmtpRpcActor时抛出。</exception>
        public static TDmtpRpcActor GetDmtpRpcActor<TDmtpRpcActor>(this IDmtpActorObject client) where TDmtpRpcActor : IDmtpRpcActor
        {
            // 从client中尝试获取DmtpRpcActor实例
            var dmtpRpcActor = client.DmtpActor.GetDmtpRpcActor();
            // 如果获取失败，则抛出ArgumentNullException异常，提示DmtpRpcActor参数为空
            if (dmtpRpcActor is null)
            {
                throw new ArgumentNullException(nameof(dmtpRpcActor), TouchSocketDmtpResource.DmtpRpcActorArgumentNull);
            }
            // 将获取到的DmtpRpcActor实例转换为TDmtpRpcActor类型并返回
            return (TDmtpRpcActor)dmtpRpcActor;
        }
        /// <summary>
        /// 向<see cref="DmtpActor"/>中设置<see cref="IDmtpRpcActor"/>
        /// </summary>
        /// <param name="dmtpActor">要设置的<see cref="IDmtpRpcActor"/>所在的<see cref="DmtpActor"/></param>
        /// <param name="dmtpRpcActor">要设置的<see cref="IDmtpRpcActor"/>实例</param>
        internal static void SetDmtpRpcActor(this IDmtpActor dmtpActor, IDmtpRpcActor dmtpRpcActor)
        {
            // 使用反射机制将dmtpRpcActor设置到dmtpActor中
            dmtpActor.SetValue(DmtpRpcActorProperty, dmtpRpcActor);
        }

        #region 插件扩展

        /// <summary>
        /// 使用DmtpRpc插件
        /// </summary>
        /// <param name="pluginManager">插件管理器实例</param>
        /// <returns>返回DmtpRpcFeature实例</returns>
#if NET6_0_OR_GREATER
        public static DmtpRpcFeature UseDmtpRpc(this IPluginManager pluginManager)
#else
        public static DmtpRpcFeature UseDmtpRpc(this IPluginManager pluginManager)
#endif
        {
            // 添加DmtpRpcFeature到插件管理器中，并返回其实例
            return pluginManager.Add<DmtpRpcFeature>();
        }

        /// <summary>
        /// 使用自定义的DmtpRpc插件。
        /// </summary>
        /// <param name="pluginManager">插件管理器，用于管理插件。</param>
        /// <returns>返回配置的DmtpRpcFeature实例。</returns>
#if NET6_0_OR_GREATER
        public static DmtpRpcFeature UseDmtpRpc<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDmtpRpcFeature>(this IPluginManager pluginManager) where TDmtpRpcFeature : DmtpRpcFeature
#else
        public static DmtpRpcFeature UseDmtpRpc<TDmtpRpcFeature>(this IPluginManager pluginManager) where TDmtpRpcFeature : DmtpRpcFeature
#endif

        {
            // 添加并返回自定义的DmtpRpc插件
            return pluginManager.Add<TDmtpRpcFeature>();
        }
        #endregion 插件扩展
    }
}