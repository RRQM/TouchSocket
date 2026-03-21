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
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// Http服务器终端接口
/// </summary>
public interface IHttpSessionClient : IHttpSession, ISessionClient, ITcpListenableClient, IOnlineClient
{
    /// <summary>
    /// 当该连接是WebSocket时，可获取该对象，否则为<see langword="null"/>。
    /// </summary>
    IWebSocket WebSocket { get; }

    /// <summary>
    /// 转化Protocol协议标识为<see cref="Protocol.WebSocket"/>
    /// </summary>
    /// <param name="autoReceive">是否启用自动接收。为<see langword="true"/>时，框架将自动驱动WebSocket数据接收循环；为<see langword="false"/>时，需调用者自行从<see cref="IWebSocket"/>读取数据。</param>
    Task<Result> SwitchProtocolToWebSocketAsync(bool autoReceive);

    /// <summary>
    /// 检验当前HTTP请求是否包含升级协议，若包含则退出HTTP解析并返回<see cref="ITransport"/>以供自定义协议直接读写。
    /// </summary>
    /// <returns>若请求包含升级协议头，则返回成功结果，<see cref="Result{T}.Value"/>为当前传输对象；否则返回失败结果。</returns>
    Task<Result<ITransport>> SwitchProtocolAsync();
}