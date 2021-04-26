using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 字节数组扩展类
    /// </summary>
    public static class BytesHelper
    {
        /// <summary>
        /// 索引包含数组
        /// </summary>
        /// <param name="srcByteArray"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="subByteArray"></param>
        /// <returns></returns>
        public static List<int> IndexOfInclude(this byte[] srcByteArray,int offset, int length, byte[] subByteArray)
        {
            int subByteArrayLen = subByteArray.Length;
            List<int> indexes = new List<int>();
            if (length < subByteArrayLen)
            {
                return indexes;
            }
            int hitLength = 0;
            for (int i = offset; i < length; i++)
            {
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
                else
                {
                    hitLength = 0;
                }

                if (hitLength == subByteArray.Length)
                {
                    hitLength = 0;
                    indexes.Add(i);
                }
            }

            return indexes;
        }


        /// <summary>
        /// 索引第一个包含数组
        /// </summary>
        /// <param name="srcByteArray"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="subByteArray"></param>
        /// <returns></returns>
        public static int IndexOfFirst(this byte[] srcByteArray, int offset, int length, byte[] subByteArray)
        {
            int subByteArrayLen = subByteArray.Length;
            if (length < subByteArrayLen)
            {
                return -1;
            }
            int hitLength = 0;
            for (int i = offset; i < length; i++)
            {
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
                else
                {
                    hitLength = 0;
                }

                if (hitLength == subByteArray.Length)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
