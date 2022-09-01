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
using System;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Data.Security;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Serialization;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// RpcActorGroup
    /// </summary>
    public class RpcActorGroup
    {
        #region 委托

        /// <summary>
        /// 获取调用函数
        /// </summary>
        public Func<string, MethodInstance> GetInvokeMethod { get; set; }

        /// <summary>
        /// 请求关闭
        /// </summary>
        public Action<RpcActor, string> OnClose { get; set; }

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        public Action<RpcActor, FileTransferStatusEventArgs> OnFileTransfered { get; set; }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        public Action<RpcActor, FileOperationEventArgs> OnFileTransfering { get; set; }

        /// <summary>
        /// 查找其他RpcActor
        /// </summary>
        public Func<string, RpcActor> OnFindRpcActor { get; set; }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        public Action<RpcActor, VerifyOptionEventArgs> OnHandshaked { get; set; }

        /// <summary>
        /// 握手
        /// </summary>
        public Action<RpcActor, VerifyOptionEventArgs> OnHandshaking { get; set; }

        /// <summary>
        /// 接收到数据
        /// </summary>
        public Action<RpcActor, short, ByteBlock> OnReceived { get; set; }

        /// <summary>
        /// 重设ID
        /// </summary>
        public Action<RpcActor, WaitSetID> OnResetID { get; set; }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        public Action<RpcActor, StreamStatusEventArgs> OnStreamTransfered { get; set; }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        public Action<RpcActor, StreamOperationEventArgs> OnStreamTransfering { get; set; }

        /// <summary>
        /// 发送数据接口
        /// </summary>
        public Action<RpcActor, bool, ArraySegment<byte>[]> OutputSend { get; set; }

        #endregion 委托

        /// <summary>
        /// 配置
        /// </summary>
        public TouchSocketConfig Config { get; set; }

        /// <summary>
        /// RpcStore
        /// </summary>
        public RpcStore RpcStore { get; set; }

        /// <summary>
        /// 创建RpcActor
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public RpcActor CreateRpcActor(object caller)
        {
            RpcActor rpcActor = new RpcActor(true)
            {
                Logger = this.Config.Container.Resolve<ILog>(),
                Caller = caller,
                RpcStore = this.RpcStore,
                OnHandshaking = this.OnHandshaking,
                GetInvokeMethod = this.GetInvokeMethod,
                OnHandshaked = this.OnHandshaked,
                OutputSend = this.OutputSend,
                OnResetID = this.OnResetID,
                OnReceived = this.OnReceived,
                OnClose = this.OnClose,
                OnFindRpcActor = this.OnFindRpcActor,
                OnStreamTransfering = this.OnStreamTransfering,
                OnStreamTransfered = this.OnStreamTransfered,
                OnFileTransfering = this.OnFileTransfering,
                OnFileTransfered = this.OnFileTransfered,
                ResponseType = this.Config.GetValue<ResponseType>(TouchRpcConfigExtensions.ResponseTypeProperty),
                RootPath = this.Config.GetValue<string>(TouchRpcConfigExtensions.RootPathProperty),
                SerializationSelector = this.Config.GetValue<SerializationSelector>(TouchRpcConfigExtensions.SerializationSelectorProperty)
            };
            return rpcActor;
        }
    }
}