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
namespace RpcProxy
{
    public interface IMyRpcServer : TouchSocket.Rpc.IRemoteServer
    {
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Boolean Login(System.String account, System.String password, InvokeOption invokeOption = default);
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Boolean> LoginAsync(System.String account, System.String password, InvokeOption invokeOption = default);

        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Boolean Register(RegisterModel register, InvokeOption invokeOption = default);
        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Boolean> RegisterAsync(RegisterModel register, InvokeOption invokeOption = default);

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Int32 Performance(System.Int32 a, InvokeOption invokeOption = default);
        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> PerformanceAsync(System.Int32 a, InvokeOption invokeOption = default);

        ///<summary>
        ///测试out
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        System.Boolean OutBytes(System.Byte[] bytes, InvokeOption invokeOption = default);
        ///<summary>
        ///测试out
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Boolean> OutBytesAsync(System.Byte[] bytes, InvokeOption invokeOption = default);

    }
    public class MyRpcServer : IMyRpcServer
    {
        public MyRpcServer(IRpcClient client)
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
        public System.Boolean Login(System.String account, System.String password, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] @_parameters = new object[] { account, password };
            System.Boolean returnData = (System.Boolean)this.Client.Invoke("Login", typeof(System.Boolean), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///登录
        ///</summary>
        public async Task<System.Boolean> LoginAsync(System.String account, System.String password, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { account, password };
            return (System.Boolean)await this.Client.InvokeAsync("Login", typeof(System.Boolean), invokeOption, parameters);
        }

        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Boolean Register(RegisterModel register, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] @_parameters = new object[] { register };
            System.Boolean returnData = (System.Boolean)this.Client.Invoke("Register", typeof(System.Boolean), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///注册
        ///</summary>
        public async Task<System.Boolean> RegisterAsync(RegisterModel register, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { register };
            return (System.Boolean)await this.Client.InvokeAsync("Register", typeof(System.Boolean), invokeOption, parameters);
        }

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Int32 Performance(System.Int32 a, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] @_parameters = new object[] { a };
            System.Int32 returnData = (System.Int32)this.Client.Invoke("Performance", typeof(System.Int32), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///性能测试
        ///</summary>
        public async Task<System.Int32> PerformanceAsync(System.Int32 a, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a };
            return (System.Int32)await this.Client.InvokeAsync("Performance", typeof(System.Int32), invokeOption, parameters);
        }

        ///<summary>
        ///测试out
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public System.Boolean OutBytes(System.Byte[] bytes, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] @_parameters = new object[] { bytes };
            System.Boolean returnData = (System.Boolean)this.Client.Invoke("OutBytes", typeof(System.Boolean), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///测试out
        ///</summary>
        public async Task<System.Boolean> OutBytesAsync(System.Byte[] bytes, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { bytes };
            return (System.Boolean)await this.Client.InvokeAsync("OutBytes", typeof(System.Boolean), invokeOption, parameters);
        }

    }
    public static class MyRpcServerExtensions
    {
        ///<summary>
        ///登录
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Boolean Login<TClient>(this TClient client, System.String account, System.String password, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] @_parameters = new object[] { account, password };
            System.Boolean returnData = (System.Boolean)client.Invoke("Login", typeof(System.Boolean), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///登录
        ///</summary>
        public static async Task<System.Boolean> LoginAsync<TClient>(this TClient client, System.String account, System.String password, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] parameters = new object[] { account, password };
            return (System.Boolean)await client.InvokeAsync("Login", typeof(System.Boolean), invokeOption, parameters);
        }

        ///<summary>
        ///注册
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Boolean Register<TClient>(this TClient client, RegisterModel register, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] @_parameters = new object[] { register };
            System.Boolean returnData = (System.Boolean)client.Invoke("Register", typeof(System.Boolean), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///注册
        ///</summary>
        public static async Task<System.Boolean> RegisterAsync<TClient>(this TClient client, RegisterModel register, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] parameters = new object[] { register };
            return (System.Boolean)await client.InvokeAsync("Register", typeof(System.Boolean), invokeOption, parameters);
        }

        ///<summary>
        ///性能测试
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Int32 Performance<TClient>(this TClient client, System.Int32 a, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] @_parameters = new object[] { a };
            System.Int32 returnData = (System.Int32)client.Invoke("Performance", typeof(System.Int32), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///性能测试
        ///</summary>
        public static async Task<System.Int32> PerformanceAsync<TClient>(this TClient client, System.Int32 a, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] parameters = new object[] { a };
            return (System.Int32)await client.InvokeAsync("Performance", typeof(System.Int32), invokeOption, parameters);
        }

        ///<summary>
        ///测试out
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        public static System.Boolean OutBytes<TClient>(this TClient client, System.Byte[] bytes, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] @_parameters = new object[] { bytes };
            System.Boolean returnData = (System.Boolean)client.Invoke("OutBytes", typeof(System.Boolean), invokeOption, @_parameters);
            return returnData;
        }
        ///<summary>
        ///测试out
        ///</summary>
        public static async Task<System.Boolean> OutBytesAsync<TClient>(this TClient client, System.Byte[] bytes, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Rpc.IRpcClient
        {
            object[] parameters = new object[] { bytes };
            return (System.Boolean)await client.InvokeAsync("OutBytes", typeof(System.Boolean), invokeOption, parameters);
        }

    }
    public class RegisterModel
    {
        public System.String Account { get; set; }
        public System.String Password { get; set; }
        public System.Int32 Id { get; set; }
    }

}
