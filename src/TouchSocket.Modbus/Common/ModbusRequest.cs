using System;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus请求类
    /// </summary>
    public class ModbusRequest : IModbusRequest
    {
        /// <summary>
        /// Modbus请求类
        /// </summary>
        public ModbusRequest()
        {
        }

        /// <summary>
        /// 使用一个功能码初始化
        /// </summary>
        /// <param name="functionCode"></param>
        public ModbusRequest(FunctionCode functionCode) : this(1, functionCode)
        {
        }

        /// <summary>
        /// 初始化一个读取类请求
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="functionCode"></param>
        /// <param name="startingAddress"></param>
        /// <param name="quantity"></param>
        public ModbusRequest(byte slaveId, FunctionCode functionCode, ushort startingAddress, ushort quantity)
        {
            this.SlaveId = slaveId;
            this.FunctionCode = functionCode;
            this.StartingAddress = startingAddress;
            this.Quantity = quantity;
        }

        /// <summary>
        /// 初始化一个站点与功能码请求。
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="functionCode"></param>
        public ModbusRequest(byte slaveId, FunctionCode functionCode)
        {
            this.SlaveId = slaveId;
            this.FunctionCode = functionCode;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 功能码
        /// </summary>
        public FunctionCode FunctionCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public ushort Quantity { get; set; }

        /// <summary>
        /// 站点号（单元标识符）
        /// </summary>
        public byte SlaveId { get; set; }

        /// <summary>
        /// 起始位置
        /// </summary>
        public ushort StartingAddress { get; set; }

        /// <summary>
        /// 在读起始位置。
        /// </summary>
        public ushort ReadStartAddress { get; set; }

        /// <summary>
        /// 读取长度
        /// </summary>
        public ushort ReadQuantity { get; set; }

        /// <summary>
        /// 设置<see cref="Data"/>的值为一个bool。
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value)
        {
            this.Data = value ? new byte[] { 255, 0 } : new byte[] { 0, 0 };
        }

        /// <summary>
        /// 设置<see cref="Data"/>的值为数组，同时设置<see cref="Quantity"/>的数量（即数组长度的1/2）。
        /// </summary>
        /// <param name="bytes"></param>
        public void SetValue(byte[] bytes)
        {
            this.Data = bytes;
            this.Quantity = (ushort)(bytes.Length / 2);
        }

        /// <summary>
        /// 设置<see cref="Data"/>的值为short。
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(short value)
        {
            this.Data = TouchSocketBitConverter.BigEndian.GetBytes(value);
        }

        /// <summary>
        /// 设置<see cref="Data"/>的值为ushort。
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(ushort value)
        {
            this.Data = TouchSocketBitConverter.BigEndian.GetBytes(value);
        }

        /// <summary>
        /// 设置<see cref="Data"/>的值为bool数组，同时设置<see cref="Quantity"/>的数量（即数组长度）。
        /// </summary>
        /// <param name="values"></param>
        public void SetValue(bool[] values)
        {
            this.Data = TouchSocketBitConverter.Default.GetBytes(values);
            this.Quantity = (ushort)values.Length;
        }
    }
}