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

using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;


/// <summary>
/// 定义了一个泛型接口<see cref="ITcpDmtpService{TClient}"/>，它继承自<see cref="ITcpDmtpServiceBase"/>和<see cref="ITcpServiceBase{TClient}"/>，
/// 其中 TClient 必须是 ITcpDmtpSessionClient 的实现。
/// 该接口用于提供基于 TCP 协议的 Dmtp 服务，支持泛型客户端类型 TClient。
/// </summary>
/// <typeparam name="TClient">客户端类型，必须实现 ITcpDmtpSessionClient 接口。</typeparam>
public interface ITcpDmtpService<TClient> : ITcpDmtpServiceBase, ITcpServiceBase<TClient> where TClient : ITcpDmtpSessionClient
{
}

/// <summary>
/// 定义了一个非泛型接口 ITcpDmtpService，它是 ITcpDmtpService 的泛型版本，
/// 其中 TClient 被固定为 TcpDmtpSessionClient 类型。
/// 该接口用于提供基于 TCP 协议的 Dmtp 服务，使用默认的 TcpDmtpSessionClient 作为客户端类型。
/// </summary>
public interface ITcpDmtpService : ITcpDmtpService<TcpDmtpSessionClient>
{
}