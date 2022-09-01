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

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 若汝棋茗所有事件基类
    /// </summary>
    public class TouchSocketEventArgs : EventArgs
    {
        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool IsPermitOperation
        {
            get => this.Operation.HasFlag(Operation.Permit);
            set
            {
                if (value)
                {
                    this.AddOperation(Operation.Permit);
                }
                else
                {
                    this.RemoveOperation(Operation.Permit);
                }
            }
        }

        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool Handled
        {
            get => this.Operation.HasFlag(Operation.Handled);
            set
            {
                if (value)
                {
                    this.AddOperation(Operation.Handled);
                }
                else
                {
                    this.RemoveOperation(Operation.Handled);
                }
            }
        }

        /// <summary>
        /// 操作类型。
        /// </summary>
        public Operation Operation { get; private set; }

        /// <summary>
        /// 添加操作
        /// </summary>
        /// <param name="operation"></param>
        public void AddOperation(Operation operation)
        {
            this.Operation |= operation;
        }

        /// <summary>
        /// 移除操作
        /// </summary>
        /// <param name="operation"></param>
        public void RemoveOperation(Operation operation)
        {
            if (this.Operation.HasFlag(operation))
            {
                this.Operation ^= operation;
            }
        }
    }
}