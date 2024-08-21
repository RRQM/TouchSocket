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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 接收数据事件参数类，继承自ByteBlockEventArgs
    /// 用于封装接收到的数据和相关的请求信息
    /// </summary>
    public class ReceivedDataEventArgs : ByteBlockEventArgs
    {

        /// <summary>
        /// 构造函数，初始化接收到的数据和请求信息
        /// </summary>
        /// <param name="byteBlock">接收到的数据块</param>
        /// <param name="requestInfo">请求信息，描述了数据接收的上下文</param>
        public ReceivedDataEventArgs(ByteBlock byteBlock, IRequestInfo requestInfo) : base(byteBlock)
        {
            this.RequestInfo = requestInfo;
        }

        /// <summary>
        /// 获取请求信息
        /// </summary>
        /// <remarks>
        /// 该属性只读，用于提供接收数据时的请求上下文信息
        /// </remarks>
        public IRequestInfo RequestInfo { get; }
    }
}