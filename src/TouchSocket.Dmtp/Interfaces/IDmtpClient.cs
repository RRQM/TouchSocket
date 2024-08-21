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

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 定义了IDmtpClient接口，它继承了多个与DMTP客户端行为相关的接口。
    /// 这些接口共同定义了客户端在系统中的行为和职责，包括但不限于客户端的连接、配置、状态管理等。
    /// </summary>
    public interface IDmtpClient : IDmtpActorObject, IClient, IClosableClient, ISetupConfigObject, IConnectableClient, IIdClient, IOnlineClient
    {
    }
}