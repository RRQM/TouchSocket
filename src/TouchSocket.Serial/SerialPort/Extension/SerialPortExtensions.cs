
using System.IO.Ports;

namespace TouchSocket.Serial
{

    /// <summary>
    /// SocketExtension
    /// </summary>
    public static class SerialPortExtensions
    {
        /// <summary>
        /// 会使用同步锁，保证所有数据上缓存区。
        /// </summary>
        /// <param name="serialPort"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public static void AbsoluteSend(this SerialPort serialPort, byte[] buffer, int offset, int length)
        {
            lock (serialPort)
            {
                serialPort.Write(buffer, offset, length);
            }
        }

        /// <summary>
        /// 尝试关闭<see cref="SerialPort"/>。不会抛出异常。
        /// </summary>
        /// <param name="serialPort"></param>
        public static void TryClose(this SerialPort serialPort)
        {
            lock (serialPort)
            {
                try
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                }
                catch
                {

                }
            }
        }

    }
}