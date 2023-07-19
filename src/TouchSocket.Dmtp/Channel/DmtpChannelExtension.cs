using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Dmtp
{
    public static class DmtpChannelExtension
    {
        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        public static void Write(this IDmtpChannel channel, byte[] data)
        {
            channel.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        public static Task WriteAsync(this IDmtpChannel channel, byte[] data)
        {
            return channel.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 尝试写入。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool TryWrite(this IDmtpChannel channel, byte[] data, int offset, int length)
        {
            if (channel.CanWrite)
            {
                try
                {
                    channel.Write(data, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试写入
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryWrite(this IDmtpChannel channel, byte[] data)
        {
            return TryWrite(channel, data, 0, data.Length);
        }

        /// <summary>
        /// 异步尝试写入
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static async Task<bool> TryWriteAsync(this IDmtpChannel channel, byte[] data, int offset, int length)
        {
            if (channel.CanWrite)
            {
                try
                {
                    await channel.WriteAsync(data, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 异步尝试写入
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Task<bool> TryWriteAsync(this IDmtpChannel channel, byte[] data)
        {
            return TryWriteAsync(channel, data, 0, data.Length);
        }
    }
}
