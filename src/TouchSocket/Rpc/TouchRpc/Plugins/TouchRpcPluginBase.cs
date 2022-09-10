//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc.Plugins
{
    /// <summary>
    /// TouchRpc插件基类
    /// </summary>
    public class TouchRpcPluginBase : TouchRpcPluginBase<ITouchRpc>
    {

    }

    /// <summary>
    /// TouchRpc插件基类
    /// </summary>
    public class TouchRpcPluginBase<TClient> : DisposableObject, ITouchRpcPlugin
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }
        #region 虚函数

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfered(TClient client, FileTransferStatusEventArgs e)
        {
        }

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnFileTransferedAsync(TClient client, FileTransferStatusEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfering(TClient client, FileOperationEventArgs e)
        {
        }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnFileTransferingAsync(TClient client, FileOperationEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在完成握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(TClient client, MsgEventArgs e)
        {
        }

        /// <summary>
        /// 在完成握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnHandshakedAsync(TClient client, VerifyOptionEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaking(TClient client, VerifyOptionEventArgs e)
        {
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnHandshakingAsync(TClient client, VerifyOptionEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 收到协议数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnReceivedProtocolData(TClient client, ProtocolDataEventArgs e)
        {
        }

        /// <summary>
        /// 收到协议数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnReceivedProtocolDataAsync(TClient client, ProtocolDataEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfered(TClient client, StreamStatusEventArgs e)
        {
        }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnStreamTransferedAsync(TClient client, StreamStatusEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfering(TClient client, StreamOperationEventArgs e)
        {
        }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnStreamTransferingAsync(TClient client, StreamOperationEventArgs e)
        {
            return Task.FromResult(0);
        }

        #endregion 虚函数

        void ITouchRpcPlugin.OnFileTransfered(ITouchRpc client, FileTransferStatusEventArgs e)
        {
            this.OnFileTransfered((TClient)client, e);
        }

        Task ITouchRpcPlugin.OnFileTransferedAsync(ITouchRpc client, FileTransferStatusEventArgs e)
        {
            return this.OnFileTransferedAsync((TClient)client, e);
        }

        void ITouchRpcPlugin.OnFileTransfering(ITouchRpc client, FileOperationEventArgs e)
        {
            this.OnFileTransfering((TClient)client, e);
        }

        Task ITouchRpcPlugin.OnFileTransferingAsync(ITouchRpc client, FileOperationEventArgs e)
        {
            return this.OnFileTransferingAsync((TClient)client, e);
        }

        void ITouchRpcPlugin.OnHandshaked(ITouchRpc client, VerifyOptionEventArgs e)
        {
            this.OnHandshaked((TClient)client, e);
        }

        Task ITouchRpcPlugin.OnHandshakedAsync(ITouchRpc client, VerifyOptionEventArgs e)
        {
            return this.OnHandshakedAsync((TClient)client, e);
        }

        void ITouchRpcPlugin.OnHandshaking(ITouchRpc client, VerifyOptionEventArgs e)
        {
            this.OnHandshaking((TClient)client, e);
        }

        Task ITouchRpcPlugin.OnHandshakingAsync(ITouchRpc client, VerifyOptionEventArgs e)
        {
            return this.OnHandshakingAsync((TClient)client, e);
        }

        void ITouchRpcPlugin.OnReceivedProtocolData(ITouchRpc client, ProtocolDataEventArgs e)
        {
            this.OnReceivedProtocolData((TClient)client, e);
        }

        Task ITouchRpcPlugin.OnReceivedProtocolDataAsync(ITouchRpc client, ProtocolDataEventArgs e)
        {
            return this.OnReceivedProtocolDataAsync((TClient)client, e);
        }

        void ITouchRpcPlugin.OnStreamTransfered(ITouchRpc client, StreamStatusEventArgs e)
        {
            this.OnStreamTransfered((TClient)client, e);
        }

        Task ITouchRpcPlugin.OnStreamTransferedAsync(ITouchRpc client, StreamStatusEventArgs e)
        {
            return this.OnStreamTransferedAsync((TClient)client, e);
        }

        void ITouchRpcPlugin.OnStreamTransfering(ITouchRpc client, StreamOperationEventArgs e)
        {
            this.OnStreamTransfering((TClient)client, e);
        }

        Task ITouchRpcPlugin.OnStreamTransferingAsync(ITouchRpc client, StreamOperationEventArgs e)
        {
            return this.OnStreamTransferingAsync((TClient)client, e);
        }
    }
}