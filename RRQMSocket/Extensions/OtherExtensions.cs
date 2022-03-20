using RRQMCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Extensions
{
    /// <summary>
    /// 其他扩展
    /// </summary>
    public static class OtherExtensions
    {
        /// <summary>
        /// 转为ResultCode
        /// </summary>
        /// <param name="channelStatus"></param>
        /// <returns></returns>
        public static ResultCode ToResultCode(this ChannelStatus channelStatus)
        {
            switch (channelStatus)
            {
                case ChannelStatus.Default:
                    return ResultCode.Default;

                case ChannelStatus.Overtime:
                    return ResultCode.Overtime;

                case ChannelStatus.Cancel:
                    return ResultCode.Canceled;

                case ChannelStatus.Completed:
                    return ResultCode.Success;

                case ChannelStatus.Moving:
                case ChannelStatus.Disposed:
                default:
                    return ResultCode.Error;
            }
        }
    }
}
