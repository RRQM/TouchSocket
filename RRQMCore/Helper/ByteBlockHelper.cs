using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore.Helper
{
    /// <summary>
    /// 字节块扩展
    /// </summary>
    public static class ByteBlockHelper
    {
        /// <summary>
        /// 转utf-8字符串
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToUtf8String(this ByteBlock byteBlock,int offset,int length)
        {
            return Encoding.UTF8.GetString(byteBlock.Buffer,offset,length);
        }
    }
}
