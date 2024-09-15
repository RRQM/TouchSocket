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

using System.Net;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 表示UDP接收结果的接口，继承自通用的接收结果接口。
    /// 该接口提供了特定于UDP接收操作的功能。
    /// </summary>
    public interface IUdpReceiverResult : IReceiverResult
    {
        /// <summary>
        /// 获取UDP端点信息，该属性标识了数据报接收的远端点。
        /// </summary>
        EndPoint EndPoint { get; }
    }
}