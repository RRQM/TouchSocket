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
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Rpc.TouchRpc.AspNetCore
{
    /// <summary>
    /// WSTouchRpcService服务器接口
    /// </summary>
    public interface IWSTouchRpcService : ITouchRpcService, IPlguinObject
    {
        /// <summary>
        /// 配置项
        /// </summary>
        TouchSocketConfig Config { get; }

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        event TransferFileEventHandler<WSTouchRpcSocketClient> FileTransfered;

        /// <summary>
        /// 文件传输开始之前
        /// </summary>
        event FileOperationEventHandler<WSTouchRpcSocketClient> FileTransfering;

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        event VerifyOptionEventHandler<WSTouchRpcSocketClient> Handshaked;

        /// <summary>
        /// 表示即将握手
        /// </summary>
        event VerifyOptionEventHandler<WSTouchRpcSocketClient> Handshaking;

        /// <summary>
        /// 收到数据
        /// </summary>
        event ProtocolReceivedEventHandler<WSTouchRpcSocketClient> Received;

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        event StreamStatusEventHandler<WSTouchRpcSocketClient> StreamTransfered;

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        event StreamOperationEventHandler<WSTouchRpcSocketClient> StreamTransfering;

        /// <summary>
        /// 断开连接
        /// </summary>
        event ClientDisconnectedEventHandler<WSTouchRpcSocketClient> Disconnected;

        /// <summary>
        /// 连接验证口令
        /// </summary>
        string VerifyToken { get; }

        /// <summary>
        /// 转换客户端
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        Task SwitchClientAsync(System.Net.WebSockets.WebSocket webSocket);

        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        void ResetID(string oldID, string newID);
    }
}