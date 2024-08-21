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

using TouchSocket.Http;

namespace TouchSocket.Dmtp
{

    /// <summary>
    /// 定义了一个接口，用于通过HTTP协议进行会话管理，并支持DMTP（动态消息传输协议）会话操作。
    /// 这个接口扩展了现有的HTTP会话客户端和会话接口，增加了对DMTP协议的支持，使得会话管理更加灵活和高效。
    /// </summary>
    public interface IHttpDmtpSessionClient : IHttpSessionClient, IHttpSession, IDmtpActorObject
    {
    }
}