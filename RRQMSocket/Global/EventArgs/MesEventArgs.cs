//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Event;

namespace RRQMSocket
{
    /// <summary>
    /// 消息事件
    /// </summary>
    public class MesEventArgs : RRQMEventArgs
    {
        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="mes"></param>
        public MesEventArgs(string mes)
        {
            this.Message = mes;
        }

        /// <summary>
        /// 直接构造函数
        /// </summary>
        public MesEventArgs()
        {
        }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}