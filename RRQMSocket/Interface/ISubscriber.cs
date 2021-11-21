using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 订阅接口
    /// </summary>
    public interface ISubscriber:IDisposable
    {
        /// <summary>
        /// 客户端
        /// </summary>
        public IProtocolClient Client { get; }

        /// <summary>
        /// 能否使用
        /// </summary>
        public bool CanUse { get; }

        /// <summary>
        /// 协议
        /// </summary>
        public short Protocol { get; }
    }
}
