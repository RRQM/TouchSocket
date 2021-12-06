using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 令箭辅助类
    /// </summary>
    public abstract class TokenSocketClient : SocketClient
    {
        private RRQMWaitHandlePool<IWaitResult> waitHandlePool;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenSocketClient()
        {
            this.waitHandlePool = new RRQMWaitHandlePool<IWaitResult>();
        }


        /// <summary>
        /// 等待返回池
        /// </summary>
        public RRQMWaitHandlePool<IWaitResult> WaitHandlePool { get => this.waitHandlePool; }
    }
}
