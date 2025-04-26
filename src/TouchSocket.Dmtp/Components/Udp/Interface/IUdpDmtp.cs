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
/// 定义了一个UdpDmtp接口，它继承自IServiceBase, IClient和IDmtpActorObject接口。
/// 该接口的目的是为UdpDmtp通信协议提供一个标准的服务接口，使得客户端和服务端可以在分布式系统中进行交互。
/// 它结合了服务的基本特性、客户端功能以及分布式对象的交互行为。
/// </summary>
public interface IUdpDmtp : IServiceBase, IDependencyClient, IDmtpActorObject
{
}