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
using RRQMCore.Run;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /*
    若汝棋茗
    */

    /// <summary>
    /// RPC代理文件程序
    /// </summary>

    public class RPCProxyInfo : WaitResult
    {
        /// <summary>
        /// 程序名
        /// </summary>
        public string AssemblyName { get; internal set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] AssemblyData { get; internal set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// 源代码
        /// </summary>
        public List<CellCode> Codes { get; internal set; }
    }
}