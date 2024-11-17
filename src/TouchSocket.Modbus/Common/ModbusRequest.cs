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
        /// <param name="functionCode">要执行的功能码</param>
        public ModbusRequest(FunctionCode functionCode) : this(1, functionCode)
        {
        }

        /// <summary>
        /// 初始化一个读取类请求
        /// </summary>
        /// <param name="slaveId">从机设备地址</param>
        /// <param name="functionCode">功能码，用于指定请求的操作类型</param>
        /// <param name="startingAddress">起始地址，用于指定从哪个寄存器或线圈开始读取</param>
        /// <param name="quantity">数量，需要读取的寄存器或线圈的数量</param>
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
        /// <param name="slaveId">站点ID，用于标识从设备。</param>
        /// <param name="functionCode">功能码，用于指定请求的具体功能。</param>
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
        /// <param name="value">要设置的布尔值。</param>
        public void SetValue(bool value)
        {
            // 将布尔值转换为字节数组，以便进行后续的通信或存储操作。
            this.Data = TouchSocketModbusUtility.BoolToBytes(value);
        }
        /// <summary>
        /// 设置<see cref="Data"/>的值为数组，同时设置<see cref="Quantity"/>的数量（即数组长度的1/2）。
        /// </summary>
        /// <param name="bytes">要设置的字节数组</param>
        public void SetValue(byte[] bytes)
        {
            // 将输入的字节数组赋值给Data属性
            this.Data = bytes;
            // 根据字节数组的长度计算Quantity属性的值，因为每个数据项假设占用2个字节，所以Quantity是数组长度的一半
            this.Quantity = (ushort)(bytes.Length / 2);
        }

        /// <summary>
        /// 设置<see cref="Data"/>的值为short。
        /// </summary>
        /// <param name="value">要设置的值</param>
        public void SetValue(short value)
        {
            // 将传入的short类型值转换为字节数组，并根据BigEndian规则进行存储
            this.Data = TouchSocketBitConverter.BigEndian.GetBytes(value);
        }

        /// <summary>
        /// 设置<see cref="Data"/>的值为ushort。
        /// </summary>
        /// <param name="value">要设置的值</param>
        public void SetValue(ushort value)
        {
            // 将传入的ushort值转换为字节数组，并根据BigEndian规则进行存储
            this.Data = TouchSocketBitConverter.BigEndian.GetBytes(value);
        }

        /// <summary>
        /// 设置<see cref="Data"/>的值为bool数组，同时设置<see cref="Quantity"/>的数量（即数组长度）。
        /// </summary>
        /// <param name="values">要设置的bool数组</param>
        public void SetValue(bool[] values)
        {
            // 将bool数组转换为字节，并赋值给Data属性
            this.Data = TouchSocketBitConverter.Default.GetBytes(values);
            // 设置Quantity属性为数组的长度，以记录bool值的数量
            this.Quantity = (ushort)values.Length;
        }
    }
}