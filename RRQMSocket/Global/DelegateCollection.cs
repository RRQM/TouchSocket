//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMSocket;
using RRQMSocket.FileTransfer;
using System.Net;

/// <summary>
/// 显示信息
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMMessageEventHandler(object sender, MesEventArgs e);

/// <summary>
/// 字节数据
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMBytesEventHandler(object sender, BytesEventArgs e);

/// <summary>
/// ByteBlock
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMByteBlockEventHandler(object sender, ByteBlock e);

/// <summary>
/// UDP ByteBlock
/// </summary>
/// <param name="remoteEndpoint"></param>
/// <param name="e"></param>
public delegate void RRQMUDPByteBlockEventHandler(EndPoint remoteEndpoint, ByteBlock e);

//public delegate void RRQMTokenPreviewConnectEventHandler(object sender, ByteBlock e);

/// <summary>
/// 传输文件处理
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMTransferFileEventHandler(object sender, TransferFileEventArgs e);

/// <summary>
/// 文件处理
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMFileEventHandler(object sender, FileEventArgs e);

/// <summary>
/// 发送文件
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMSendFileEventHandler(object sender, TransferFileArgs e);

/// <summary>
/// 完成发送
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMFileFinishedEventHandler(object sender, FileFinishedArgs e);

/// <summary>
/// 传输文件错误
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void RRQMTransferFileMessageEventHandler(object sender, TransferFileMessageArgs e);