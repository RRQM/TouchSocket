//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC调用设置
    /// </summary>
    public class InvokeOption
    {
        static InvokeOption()
        {
            _tcpInvoke = new InvokeOption();
            _tcpInvoke.WaitTime = 3;
            _tcpInvoke.Feedback = true;
            
            _udpInvoke = new InvokeOption();
            _udpInvoke.WaitTime = 3;
            _udpInvoke.Feedback = false;
        }
        private static InvokeOption _tcpInvoke;
        /// <summary>
        /// 默认设置。
        /// WaitTime=3，Feedback=True。
        /// </summary>
        public static InvokeOption CanFeedback { get { return _tcpInvoke; } } 
        
        private static InvokeOption _udpInvoke;

        /// <summary>
        /// 默认设置。
        /// WaitTime=3，Feedback=False。
        /// </summary>
        public static InvokeOption NoFeedback { get { return _udpInvoke; } }

        /// <summary>
        /// 调用等待时长
        /// </summary>
        public int WaitTime { get; set; }

        /// <summary>
        /// 调用反馈
        /// </summary>
        public bool Feedback { get; set; }


    }
}
