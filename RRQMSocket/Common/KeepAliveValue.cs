using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 保活机制
    /// </summary>
    public class KeepAliveValue
    {
        /// <summary>
        /// 保活机制
        /// </summary>
        public byte[] KeepAliveTime
        {
            get
            {
                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes(Interval).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes(AckInterval).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                return inOptionValues;
            }
        }

        /// <summary>
        /// 是否启用保活。默认为True。
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 发送间隔，默认20*1000ms
        /// </summary>
        public uint Interval { get; set; } = 20 * 1000;

        /// <summary>
        /// 确认间隔，默认2*1000ms
        /// </summary>
        public uint AckInterval { get; set; } = 2 * 1000;
    }
}
