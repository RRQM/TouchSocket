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
//using TouchSocket.Core.ByteManager;

//namespace TouchSocket.Rpc.TouchRpc
//{
//    /// <summary>
//    /// 表示即将握手
//    /// </summary>
//    /// <typeparam name="TClient"></typeparam>
//    /// <param name="client"></param>
//    /// <param name="e"></param>
//    public delegate void VerifyOptionEventHandler<TClient>(TClient client, VerifyOptionEventArgs e);

//    /// <summary>
//    /// 传输文件操作处理
//    /// </summary>
//    /// <param name="client"></param>
//    /// <param name="e"></param>
//    public delegate void FileOperationEventHandler<TClient>(TClient client, FileOperationEventArgs e);

//    /// <summary>
//    /// 协议数据
//    /// </summary>
//    /// <param name="client"></param>
//    /// <param name="protocol"></param>
//    /// <param name="byteBlock"></param>
//    public delegate void ProtocolReceivedEventHandler<TClient>(TClient client, short protocol, ByteBlock byteBlock);

//    /// <summary>
//    /// 收到流操作
//    /// </summary>
//    /// <param name="client"></param>
//    /// <param name="e"></param>
//    public delegate void StreamOperationEventHandler<TClient>(TClient client, StreamOperationEventArgs e);

//    /// <summary>
//    /// 流状态
//    /// </summary>
//    /// <param name="client"></param>
//    /// <param name="e"></param>
//    public delegate void StreamStatusEventHandler<TClient>(TClient client, StreamStatusEventArgs e) where TClient : IRpcActor;

//    /// <summary>
//    /// 传输文件消息
//    /// </summary>
//    /// <param name="client"></param>
//    /// <param name="e"></param>
//    public delegate void TransferFileEventHandler<TClient>(TClient client, FileTransferStatusEventArgs e);

//    /// <summary>
//    /// 在远程操作访问之前。
//    /// </summary>
//    /// <typeparam name="TClient"></typeparam>
//    /// <param name="client"></param>
//    /// <param name="e"></param>
//    public delegate void RemoteAccessingEventHandler<TClient>(TClient client, RemoteAccessActionEventArgs e);

//    /// <summary>
//    /// 在远程操作访问之后。
//    /// </summary>
//    /// <typeparam name="TClient"></typeparam>
//    /// <param name="client"></param>
//    /// <param name="e"></param>
//    public delegate void RemoteAccessedEventHandler<TClient>(TClient client, RemoteAccessEventArgs e);
//}