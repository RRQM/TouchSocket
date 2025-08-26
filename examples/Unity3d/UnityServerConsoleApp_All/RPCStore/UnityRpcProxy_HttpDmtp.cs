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
namespace UnityRpcProxy_HttpDmtp
{
    public interface IUnityRpcStore : TouchSocket.Rpc.IRemoteServer
    {
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        LoginModelResult DmtpRpc_Login(LoginModel model, InvokeOption invokeOption = default);
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<LoginModelResult> DmtpRpc_LoginAsync(LoginModel model, InvokeOption invokeOption = default);

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 DmtpRpc_Performance(System.Int32 i, InvokeOption invokeOption = default);
        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> DmtpRpc_PerformanceAsync(System.Int32 i, InvokeOption invokeOption = default);

    }
    public class UnityRpcStore : IUnityRpcStore
    {
        public UnityRpcStore(IRpcClient client)
        {
            this.Client = client;
        }
        public IRpcClient Client { get; private set; }
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public LoginModelResult DmtpRpc_Login(LoginModel model, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] @_parameters = new object[] { model };
            LoginModelResult returnData = (LoginModelResult)this.Client.Invoke("DmtpRpc_Login", typeof(LoginModelResult), invokeOption, @_parameters);
            return returnData;

        }
        ///<summary>
        ///登录
        ///</summary>
        public async Task<LoginModelResult> DmtpRpc_LoginAsync(LoginModel model, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { model };
            return (LoginModelResult)await this.Client.InvokeAsync("DmtpRpc_Login", typeof(LoginModelResult), invokeOption, parameters);

        }

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 DmtpRpc_Performance(System.Int32 i, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] @_parameters = new object[] { i };
            System.Int32 returnData = (System.Int32)this.Client.Invoke("DmtpRpc_Performance", typeof(System.Int32), invokeOption, @_parameters);
            return returnData;

        }
        ///<summary>
        ///性能测试
        ///</summary>
        public async Task<System.Int32> DmtpRpc_PerformanceAsync(System.Int32 i, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { i };
            return (System.Int32)await this.Client.InvokeAsync("DmtpRpc_Performance", typeof(System.Int32), invokeOption, parameters);

        }

    }
    public static class UnityRpcStoreExtensions
    {
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static LoginModelResult DmtpRpc_Login<TClient>(this TClient client, LoginModel model, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Dmtp.Rpc.IDmtpRpcActor
        {
            object[] @_parameters = new object[] { model };
            LoginModelResult returnData = (LoginModelResult)client.Invoke("DmtpRpc_Login", typeof(LoginModelResult), invokeOption, @_parameters);
            return returnData;

        }
        ///<summary>
        ///登录
        ///</summary>
        public static async Task<LoginModelResult> DmtpRpc_LoginAsync<TClient>(this TClient client, LoginModel model, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Dmtp.Rpc.IDmtpRpcActor
        {
            object[] parameters = new object[] { model };
            return (LoginModelResult)await client.InvokeAsync("DmtpRpc_Login", typeof(LoginModelResult), invokeOption, parameters);

        }

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 DmtpRpc_Performance<TClient>(this TClient client, System.Int32 i, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Dmtp.Rpc.IDmtpRpcActor
        {
            object[] @_parameters = new object[] { i };
            System.Int32 returnData = (System.Int32)client.Invoke("DmtpRpc_Performance", typeof(System.Int32), invokeOption, @_parameters);
            return returnData;

        }
        ///<summary>
        ///性能测试
        ///</summary>
        public static async Task<System.Int32> DmtpRpc_PerformanceAsync<TClient>(this TClient client, System.Int32 i, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Dmtp.Rpc.IDmtpRpcActor
        {
            object[] parameters = new object[] { i };
            return (System.Int32)await client.InvokeAsync("DmtpRpc_Performance", typeof(System.Int32), invokeOption, parameters);

        }

    }
    public class LoginModel
    {
        public System.String Token { get; set; }
        public System.String Account { get; set; }
        public System.String Password { get; set; }
    }

    public class LoginModelResult
    {
        public TouchSocket.Core.ResultCode ResultCode { get; set; }
        public System.String Message { get; set; }
    }

}
