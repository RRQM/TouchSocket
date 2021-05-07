//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC支持协议
    /// </summary>
    [Flags]
    public enum SupportProtocol
    {
        /// <summary>
        /// TCP协议
        /// </summary>
        TCP = 1,

        /// <summary>
        /// UDP协议
        /// </summary>
        UDP = 2
    }
}