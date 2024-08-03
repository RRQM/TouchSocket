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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcClientExtension
    /// </summary>
    public static class RpcClientExtension
    {
        #region RpcClient

        public static object Invoke(this IRpcClient client, string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            return client.InvokeAsync(invokeKey, returnType, invokeOption, parameters).GetFalseAwaitResult();
        }

        /// <inheritdoc cref="IRpcClient.InvokeAsync(RpcRequest)"/>
        public static T InvokeT<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)(client.Invoke(invokeKey, typeof(T), invokeOption, parameters));
        }

        /// <inheritdoc cref="IRpcClient.InvokeAsync(RpcRequest)"/>
        public static async Task<T> InvokeTAsync<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)(await client.InvokeAsync(invokeKey, typeof(T), invokeOption, parameters).ConfigureAwait(false));
        }

        #endregion RpcClient

        #region ITargetRpcClient

        public static object Invoke(this ITargetRpcClient client, string targetId, string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            return client.InvokeAsync(targetId, invokeKey, returnType, invokeOption, parameters).GetFalseAwaitResult();
        }

        /// <inheritdoc cref="ITargetRpcClient.InvokeAsync(string, string, Type, IInvokeOption, object[])"/>
        public static T InvokeT<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)(client.Invoke(targetId, invokeKey, typeof(T), invokeOption, parameters));
        }

        /// <inheritdoc cref="ITargetRpcClient.InvokeAsync(string, string, Type, IInvokeOption, object[])"/>
        public static async Task<T> InvokeTAsync<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)(await client.InvokeAsync(targetId, invokeKey, typeof(T), invokeOption, parameters));
        }

        #endregion ITargetRpcClient
    }
}