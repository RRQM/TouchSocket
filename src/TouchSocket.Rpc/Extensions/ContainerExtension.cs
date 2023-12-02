//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using TouchSocket.Core;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TouchSocket.Rpc
{
    /// <summary>
    /// ContainerExtension
    /// </summary>
    public static class ContainerExtension
    {
        /// <summary>
        /// 向容器中添加<see cref="RpcStore"/>。
        /// </summary>
        /// <param name="registrator"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegistrator AddRpcStore(this IRegistrator registrator, Action<RpcStore> action)
        {
            var rpcStore = new RpcStore(registrator);
            action.Invoke(rpcStore);
            registrator.RegisterSingleton(rpcStore);
            return registrator;
        }
    }
}