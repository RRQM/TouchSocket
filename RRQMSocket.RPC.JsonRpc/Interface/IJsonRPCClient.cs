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
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMSocket.RPC.RRQMRPC;
using System;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// Json客户端RPC接口
    /// </summary>
    public interface IJsonRPCClient : IRPCClient
    {
        /// <summary>
        /// 序列化生成器
        /// </summary>
        JsonFormatConverter JsonFormatConverter { get; }
    }
}