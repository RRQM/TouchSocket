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
