using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 允许返回的字节
    /// </summary>
    public class ReturnBytesEventArgs : BytesEventArgs
    {
        /// <summary>
        /// 返回字节
        /// </summary>
        public byte[] ReturnDataBytes { get; set; }
    }
}
