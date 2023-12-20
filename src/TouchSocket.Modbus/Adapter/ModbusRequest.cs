using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus请求类
    /// </summary>
    public class ModbusRequest:ModbusBase
    {
        /// <summary>
        /// 使用一个功能码初始化
        /// </summary>
        /// <param name="value"></param>
        public ModbusRequest(FunctionCode value)
        {
            this.SlaveId = 1;
            this.FunctionCode = value;
        }

        /// <summary>
        /// 初始化一个读取类请求
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="functionCode"></param>
        /// <param name="startingAddress"></param>
        /// <param name="quantity"></param>
        public ModbusRequest(byte slaveId, FunctionCode functionCode,ushort startingAddress,ushort quantity)
        {
            this.SlaveId = slaveId;
            this.FunctionCode = functionCode;
            this.StartingAddress = startingAddress;
            this.Quantity = quantity;
        }

        /// <summary>
        /// 初始化一个写入类请求
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="value"></param>
        /// <param name="startingAddress"></param>
        /// <param name="data"></param>
        public ModbusRequest(byte slaveId, FunctionCode value, ushort startingAddress, byte[] data)
        {
            this.SlaveId = slaveId;
            this.FunctionCode = value;
            this.Data = data;
        }

        /// <summary>
        /// 初始化一个站点与功能码请求。
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="value"></param>
        public ModbusRequest(byte slaveId, FunctionCode value)
        {
            this.SlaveId = slaveId;
            this.FunctionCode = value;
        }

        /// <summary>
        /// 设置<see cref="ModbusBase.Quantity"/>的值。
        /// </summary>
        /// <param name="value"></param>
        public void SetQuantity(ushort value)
        {
            this.Quantity = value;
        }

        /// <summary>
        /// 设置<see cref="ModbusBase.SlaveId"/>的值。
        /// </summary>
        /// <param name="value"></param>
        public void SetSlaveId(byte value)
        {
            this.SlaveId = value;
        }

        /// <summary>
        /// 设置<see cref="ModbusBase.StartingAddress"/>的值。
        /// </summary>
        /// <param name="value"></param>
        public void SetStartingAddress(ushort value)
        {
            this.StartingAddress = value;
        }

        /// <summary>
        /// 设置<see cref="ModbusBase.Data"/>的值为一个bool。
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value)
        {
            this.Data = value ? new byte[] { 255, 0 } : new byte[] { 0, 0 };
        }

        /// <summary>
        /// 设置<see cref="ModbusBase.Data"/>的值为数组，同时设置<see cref="ModbusBase.Quantity"/>的数量（即数组长度的1/2）。
        /// </summary>
        /// <param name="bytes"></param>
        public void SetValue(byte[] bytes)
        {
            this.Data = bytes;
            this.Quantity = (ushort)(bytes.Length / 2);
        }

        /// <summary>
        /// 设置<see cref="ModbusBase.Data"/>的值为short。
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(short value)
        {
            this.Data = TouchSocketBitConverter.BigEndian.GetBytes(value);
        }

        /// <summary>
        /// 设置<see cref="ModbusBase.Data"/>的值为bool数组，同时设置<see cref="ModbusBase.Quantity"/>的数量（即数组长度）。
        /// </summary>
        /// <param name="values"></param>
        public void SetValue(bool[] values)
        {
            this.Data = TouchSocketBitConverter.Default.GetBytes(values);
            this.Quantity = (ushort)values.Length;
        }
    }
}
