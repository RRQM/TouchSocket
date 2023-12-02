//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// DmtpChannelExtension
    /// </summary>
    public static class DmtpChannelExtension
    {
        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        public static void Write(this IDmtpChannel channel, byte[] data)
        {
            channel.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        public static Task WriteAsync(this IDmtpChannel channel, byte[] data)
        {
            return channel.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 尝试写入。
        /// </summary>
        /// <param name="channel"></param>
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
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryWrite(this IDmtpChannel channel, byte[] data)
        {
            return TryWrite(channel, data, 0, data.Length);
        }

        /// <summary>
        /// 异步尝试写入
        /// </summary>
        /// <param name="channel"></param>
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
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Task<bool> TryWriteAsync(this IDmtpChannel channel, byte[] data)
        {
            return TryWriteAsync(channel, data, 0, data.Length);
        }
    }
}