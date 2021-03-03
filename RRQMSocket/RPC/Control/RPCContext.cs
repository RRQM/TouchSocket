//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Run;
using System;
using System.Collections.Generic;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC传输类
    /// </summary>
    [Serializable]
    internal class RPCContext : WaitResult
    {
        /// <summary>
        /// 方法
        /// </summary>
        internal string Method { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        internal byte[] ReturnParameterBytes { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        internal List<byte[]> ParametersBytes { get; set; }
    }
}