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

using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了一个接口，用于异步发送标识符和请求信息
/// </summary>
public interface IIdRequestInfoSender
{

    /// <summary>
    /// 异步发送指定标识符和请求信息的方法
    /// </summary>
    /// <param name="id">要发送的标识符</param>
    /// <param name="requestInfo">请求信息对象，包含发送的具体内容</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，表示异步操作的完成</returns>
    Task SendAsync(string id, IRequestInfo requestInfo,CancellationToken token=default);
}