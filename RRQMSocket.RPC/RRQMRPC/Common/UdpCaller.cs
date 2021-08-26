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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// Udp调用者
    /// </summary>
    public class UdpCaller : ICaller
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="service"></param>
        /// <param name="callerEndPoint"></param>
        public UdpCaller(UdpSession service, EndPoint callerEndPoint)
        {
            this.service = service;
            this.callerEndPoint = callerEndPoint;
        }

        private UdpSession service;

        /// <summary>
        /// Udp服务器
        /// </summary>
        public UdpSession Service
        {
            get { return service; }
        }

        private EndPoint callerEndPoint;

        /// <summary>
        /// 调用者终结点
        /// </summary>
        public EndPoint CallerEndPoint
        {
            get { return callerEndPoint; }
        }

    }
}
