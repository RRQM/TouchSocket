﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器状态
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// 无状态，指示为初建
        /// </summary>
        None,

        /// <summary>
        /// 正在运行
        /// </summary>
        Running,

        /// <summary>
        /// 已停止
        /// </summary>
        Stopped,

        /// <summary>
        /// 已释放
        /// </summary>
        Disposed
    }
}