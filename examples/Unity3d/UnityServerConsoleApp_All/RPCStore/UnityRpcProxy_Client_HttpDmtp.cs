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
namespace UnityRpcProxy
{
    public interface ITouch_HttpDmtp_Client_UnityRpcStore : TouchSocket.Rpc.IRemoteServer
    {
      
        ///<summary>
        ///无注释信息
        ///</summary>
        /// <exception cref="System.TimeoutException">调用超时</exception>
        /// <exception cref="TouchSocket.Rpc.RpcInvokeException">Rpc调用异常</exception>
        /// <exception cref="System.Exception">其他异常</exception>
        Task<System.Int32> RandomNumberAsync(System.Int32 a, System.Int32 b, InvokeOption invokeOption = default);

    }
    public class Touch_HttpDmtp_Client_UnityRpcStore : ITouch_HttpDmtp_Client_UnityRpcStore
    {
        public Touch_HttpDmtp_Client_UnityRpcStore(IRpcClient client)
        {
            this.Client = client;
        }
        public IRpcClient Client { get; private set; }
       
        ///<summary>
        ///无注释信息
        ///</summary>
        public async Task<System.Int32> RandomNumberAsync(System.Int32 a, System.Int32 b, InvokeOption invokeOption = default)
        {
            if (this.Client == null)
            {
                throw new RpcException("IRpcClient为空，请先初始化或者进行赋值");
            }
            object[] parameters = new object[] { a, b };
            return (System.Int32)await this.Client.InvokeAsync("RandomNumber", typeof(System.Int32), invokeOption, parameters);

        }

    }
    public static class Touch_HttpDmtp_Client_UnityRpcStoreExtensions
    {
       
        ///<summary>
        ///无注释信息
        ///</summary>
        public static async Task<System.Int32> RandomNumberAsync<TClient>(this TClient client, System.Int32 a, System.Int32 b, InvokeOption invokeOption = default) where TClient :
        TouchSocket.Dmtp.Rpc.IDmtpRpcActor
        {
            object[] parameters = new object[] { a, b };
            return (System.Int32)await client.InvokeAsync("RandomNumber", typeof(System.Int32), invokeOption, parameters);

        }

    }
}
