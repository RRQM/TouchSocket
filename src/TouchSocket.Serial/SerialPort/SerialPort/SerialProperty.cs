
using System.ComponentModel;
using System.IO.Ports;
namespace TouchSocket.Serial
{

    /// <summary>
    /// 串口属性
    /// </summary>
    public class SerialProperty
    {
        /// <summary>
        /// COM
        /// </summary>
        [Description("COM口")]
        public string PortName { get; set; } = "COM1";
        /// <summary>
        /// 波特率
        /// </summary>
        [Description("波特率")]
        public int BaudRate { get; set; } = 9600;
        /// <summary>
        /// 数据位
        /// </summary>
        [Description("数据位")]
        public int DataBits { get; set; } = 8;
        /// <summary>
        /// 校验位
        /// </summary>
        [Description("校验位")]
        public Parity Parity { get; set; } = Parity.None;
        /// <summary>
        /// 停止位
        /// </summary>
        [Description("停止位")]
        public StopBits StopBits { get; set; } = StopBits.One;
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{PortName}[{BaudRate},{DataBits},{StopBits},{Parity}]";
        }
    }
}