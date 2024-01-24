//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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
            this.Data = TouchSocketModbusUtility.BoolToBytes(value);
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