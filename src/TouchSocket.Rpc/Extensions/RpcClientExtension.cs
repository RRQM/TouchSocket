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