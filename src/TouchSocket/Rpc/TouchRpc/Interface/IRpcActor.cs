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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// RpcActor接口
    /// </summary>
    public partial interface IRpcActor : IRpcActorBase, IRpcClient, IIDRpcActor
    {

        /// <summary>
        /// 表示是否已经完成握手连接。
        /// </summary>
        bool IsHandshaked { get; }

        /// <summary>
        /// 根路径
        /// </summary>
        string RootPath { get; set; }

        /// <summary>
        /// 允许响应远程请求的类型。
        /// </summary>
        ResponseType ResponseType { get; set; }

        /// <summary>
        /// 从对点拉取文件
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Result PullFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null);

        /// <summary>
        /// 异步从对点拉取文件
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<Result> PullFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null);

        /// <summary>
        /// 向对点推送文件
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Result PushFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null);

        /// <summary>
        /// 异步向对点推送文件
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<Result> PushFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null);


        /// <summary>
        /// 判断使用该ID的Channel是否存在。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ChannelExisted(int id);

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <returns></returns>
        Channel CreateChannel();

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="id">指定ID</param>
        /// <returns></returns>
        Channel CreateChannel(int id);

        /// <summary>
        /// 发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Result SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata = default);

        /// <summary>
        /// 异步发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<Result> SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata = default);

        /// <summary>
        /// 订阅通道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool TrySubscribeChannel(int id, out Channel channel);

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="newID"></param>
        /// <param name="cancellationToken"></param>
        void ResetID(string newID, CancellationToken cancellationToken = default);
    }
}
