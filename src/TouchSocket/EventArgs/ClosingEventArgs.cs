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
    /// 即将断开连接事件参数
    /// </summary>
    /// <remarks>
    /// 当发生断开连接事件时，使用该类来传递断开连接的消息。
    /// 继承自MsgEventArgs，用于提供断开连接时的详细信息。
    /// </remarks>
    public class ClosingEventArgs : MsgEventArgs
    {
        /// <summary>
        /// 初始化即将断开连接事件参数对象
        /// </summary>
        /// <param name="msg">断开连接的消息</param>
        public ClosingEventArgs(string msg) : base(msg)
        {
        }
    }
}