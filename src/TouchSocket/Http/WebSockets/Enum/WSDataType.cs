//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocket数据类型
    /// </summary>
    public enum WSDataType : ushort
    {
        /// <summary>
        /// 表示一个中间数据包，denotes a continuation frame
        /// </summary>
        Cont = 0,

        /// <summary>
        /// 表示一个text类型数据包
        /// </summary>
        Text = 1,

        /// <summary>
        /// 表示一个binary类型数据包
        /// </summary>
        Binary = 2,

        /// <summary>
        /// 表示一个断开连接类型数据包
        /// </summary>
        Close = 8,

        /// <summary>
        /// 表示一个ping类型数据包
        /// </summary>
        Ping = 9,

        /// <summary>
        /// 表示一个pong类型数据包
        /// </summary>
        Pong = 10
    }
}