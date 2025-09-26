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
using TouchSocket.Sockets;

namespace TouchSocket.WebApi;

/// <summary>
/// 定义了一个用于Web API客户端操作的公共接口，该接口继承自多个基接口，以支持丰富的网络和会话功能
/// </summary>
/// <remarks>
/// 此接口结合了IWebApiClientBase, IHttpSession, ISetupConfigObject, IOnlineClient和ITcpConnectableClient的功能，
/// 提供了一种综合的方法来实现Web API的客户端操作。实现这个接口的类将能够发起Web API请求，
/// 管理会话状态，配置客户端设置，处理在线状态，并支持TCP连接管理。
/// </remarks>
public interface IWebApiClient : IWebApiClientBase, IHttpSession, ISetupConfigObject, IOnlineClient, ITcpConnectableClient
{
}