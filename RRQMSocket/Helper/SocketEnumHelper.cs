using RRQMCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Helper
{
    /// <summary>
    /// 枚举值
    /// </summary>
    public static class SocketEnumHelper
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
                    return  ResultCode.Default;
                case ChannelStatus.Overtime:
                    return ResultCode.Overtime;
                case ChannelStatus.Error:
                    return ResultCode.Error;
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
