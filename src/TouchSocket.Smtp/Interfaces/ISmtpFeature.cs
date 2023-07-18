using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// SMTP功能性组件接口
    /// </summary>
    public interface ISmtpFeature
    {
        /// <summary>
        /// 起始协议
        /// </summary>
        ushort StartProtocol { get; set; }

        /// <summary>
        /// 保留协议长度
        /// </summary>
        ushort ReserveProtocolSize { get; }
    }
}
