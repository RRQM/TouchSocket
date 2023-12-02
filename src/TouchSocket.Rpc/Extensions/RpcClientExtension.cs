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
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcClientExtension
    /// </summary>
    public static class RpcClientExtension
    {
        #region RpcClient

        /// <inheritdoc cref="IRpcClient.Invoke(Type, string, IInvokeOption, object[])"/>
        public static T InvokeT<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)client.Invoke(typeof(T), invokeKey, invokeOption, parameters);
        }

        /// <inheritdoc cref="IRpcClient.Invoke(Type, string, IInvokeOption, ref object[], Type[])"/>
        public static T InvokeT<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            return (T)client.Invoke(typeof(T), invokeKey, invokeOption, ref parameters, types);
        }

        /// <inheritdoc cref="IRpcClient.InvokeAsync(Type, string, IInvokeOption, object[])"/>
        public static async Task<T> InvokeTAsync<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)await client.InvokeAsync(typeof(T), invokeKey, invokeOption, parameters);
        }

        #endregion RpcClient

        #region ITargetRpcClient

        /// <inheritdoc cref="ITargetRpcClient.Invoke(Type, string, string, IInvokeOption, object[])"/>
        public static T InvokeT<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)client.Invoke(typeof(T), targetId, invokeKey, invokeOption, parameters);
        }

        /// <inheritdoc cref="ITargetRpcClient.Invoke(Type, string, string, IInvokeOption, ref object[], Type[])"/>
        public static T InvokeT<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            return (T)client.Invoke(typeof(T), targetId, invokeKey, invokeOption, ref parameters, types);
        }

        /// <inheritdoc cref="ITargetRpcClient.InvokeAsync(Type, string, string, IInvokeOption, object[])"/>
        public static async Task<T> InvokeTAsync<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)await client.InvokeAsync(typeof(T), targetId, invokeKey, invokeOption, parameters);
        }

        #endregion ITargetRpcClient
    }
}