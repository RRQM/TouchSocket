//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace RRQMCore.Run
{
    /// <summary>
    /// 等待返回类
    /// </summary>
    [Serializable]
    public class WaitResult : IWaitResult
    {
        /// <summary>
        /// 消息
        /// </summary>
        protected string message;

        /// <summary>
        /// 标记号
        /// </summary>
        protected int sign;

        /// <summary>
        /// 状态
        /// </summary>
        protected byte status;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message
        {
            get => this.message;
            set => this.message = value;
        }

        /// <summary>
        /// 标记号
        /// </summary>
        public int Sign
        {
            get => this.sign;
            set => this.sign = value;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public byte Status
        {
            get => this.status;
            set => this.status = value;
        }
    }
}