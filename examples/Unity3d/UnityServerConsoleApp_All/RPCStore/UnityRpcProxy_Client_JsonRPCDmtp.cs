/*
此代码由Rpc工具直接生成，非必要请不要修改此处代码
*/
#pragma warning disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
namespace UnityRpcProxy
{
    public interface IReverseJsonRpcServer : TouchSocket.Rpc.IRemoteServer
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 Add(System.Int32 a, System.Int32 b, InvokeOption invokeOption = default);
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> AddAsync(System.Int32 a, System.Int32 b, InvokeOption invokeOption = default);

    }
    public class ReverseJsonRpcServer : IReverseJsonRpcServer
    {
        public ReverseJsonRpcServer(IRpcClient client)
        {
            this.Client = client;
        }
        public IRpcClient Client { get; private set; }
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 Add(System.Int32 a, System.Int32 b, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] @_parameters = new object[] { a, b };
            System.Int32 returnData = (System.Int32)this.Client.Invoke("Add", typeof(System.Int32), invokeOption, @_parameters);
            return returnData;

        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<System.Int32> AddAsync(System.Int32 a, System.Int32 b, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            return (System.Int32)await this.Client.InvokeAsync("Add", typeof(System.Int32), invokeOption, parameters);

        }

    }
    public static class ReverseJsonRpcServerExtensions
    {
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 Add<TClient>(this TClient client, System.Int32 a, System.Int32 b, InvokeOption invokeOption = default) where TClient :
        TouchSocket.JsonRpc.IJsonRpcClient
        {
            object[] @_parameters = new object[] { a, b };
            System.Int32 returnData = (System.Int32)client.Invoke("Add", typeof(System.Int32), invokeOption, @_parameters);
            return returnData;

        }
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<System.Int32> AddAsync<TClient>(this TClient client, System.Int32 a, System.Int32 b, InvokeOption invokeOption = default) where TClient :
        TouchSocket.JsonRpc.IJsonRpcClient
        {
            object[] parameters = new object[] { a, b };
            return (System.Int32)await client.InvokeAsync("Add", typeof(System.Int32), invokeOption, parameters);

        }

    }
}
