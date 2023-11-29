
using System.IO.Ports;
using System.Threading.Tasks;

using TouchSocket.Core;

namespace TouchSocket.Serial
{
    /// <summary>
    /// Connecting
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="e"></param>
    public delegate Task SerialConnectingEventHandler<TClient>(TClient client, SerialConnectingEventArgs e);

    /// <summary>
    /// 客户端连接事件。
    /// </summary>
    public class SerialConnectingEventArgs : MsgPermitEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serialPort"></param>
        public SerialConnectingEventArgs(SerialPort serialPort)
        {
            this.SerialPort = serialPort;
            this.IsPermitOperation = true;
        }

        /// <summary>
        /// 客户端Id。该Id的赋值，仅在服务器适用。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 新初始化的通信器
        /// </summary>
        public SerialPort SerialPort { get; private set; }
    }
}
