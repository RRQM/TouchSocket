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

using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 接收结果接口，定义了接收操作完成后所需满足的方法和属性
    /// </summary>
    public interface IReceiverResult : IDisposable
    {
        /// <summary>
        /// 获取接收到的数据字节块
        /// </summary>
        ByteBlock ByteBlock { get; }

        /// <summary>
        /// 获取与接收数据相关的请求信息
        /// </summary>
        IRequestInfo RequestInfo { get; }

        /// <summary>
        /// 获取接收操作是否已完成的标志
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// 获取接收操作相关的消息字符串，例如错误信息或完成状态描述
        /// </summary>
        string Message { get; }
    }
}