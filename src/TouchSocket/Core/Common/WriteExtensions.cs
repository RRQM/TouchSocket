using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// WriteExtensions
    /// </summary>
    public static class WriteExtensions
    {
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static void Write<T>(this T writer, byte[] buffer) where T : IWrite
        {
            writer.Write(buffer, 0, buffer.Length);
        }
    }
}
