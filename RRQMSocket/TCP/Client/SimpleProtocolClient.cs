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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议客户端
    /// </summary>
    public class SimpleProtocolClient : ProtocolClient
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public event Action<short?, ByteBlock> Received;

        /// <summary>
        /// 处理协议数据
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected sealed override void HandleProtocolData(short? procotol, ByteBlock byteBlock)
        {
            this.Received?.Invoke(procotol, byteBlock);
        }
    }
}
