using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 创建通道
    /// </summary>
    public class WaitCreateChannel : WaitResult
    {
        /// <summary>
        /// 随机ID
        /// </summary>
        public bool RandomID { get; set; }

        /// <summary>
        /// 通道ID
        /// </summary>
        public int ChannelID { get; set; }

        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientID { get; set; }
    }
}
