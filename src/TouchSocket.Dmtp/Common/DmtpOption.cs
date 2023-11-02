using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 针对Dmtp的配置项
    /// </summary>
    public class DmtpOption
    {
        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken { get; set; }

        /// <summary>
        /// 连接时指定Id。
        /// <para>
        /// 使用该功能时，仅在服务器的Handshaking之后生效。且如果id重复，则会连接失败。
        /// </para>
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 设置DmtpClient连接时的元数据
        /// </summary>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// 验证连接超时时间。仅用于服务器。意为：当服务器收到基础链接，在指定的时间内如果没有收到握手信息，则直接视为无效链接，直接断开。
        /// </summary>
        public TimeSpan VerifyTimeout { get; set; } = TimeSpan.FromSeconds(3);
    }
}
