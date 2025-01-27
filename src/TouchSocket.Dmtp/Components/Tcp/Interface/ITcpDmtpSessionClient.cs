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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 定义了一个接口，用于通过TCP协议实现的Dmtp会话客户端。
/// 它扩展了ITcpSession、ITcpListenableClient、IResolverObject、IDmtpActorObject和ISessionClient接口，
/// 提供了一种机制，使得客户端能够参与基于TCP的Dmtp会话，包括监听、解析和会话管理功能。
/// </summary>
public interface ITcpDmtpSessionClient : ITcpSession, ITcpListenableClient, IResolverObject, IDmtpActorObject, ISessionClient
{
}